namespace LogFlowPipeline.Tests;

using Xunit;
using LogFlowPipeline;

public class PipelineErrorTests
{
    private const string Good = "1.1.1.1 - - [10/Oct/2026:13:55:36 +0300] \"GET /a HTTP/1.0\" 200 10";
    private const string Bad = "garbage";

    private class ListSource : ISource<string>
    {
        private readonly string[] _lines;
        public ListSource(params string[] lines) => _lines = lines;
        public void Produce(IEmitter<string> o) { foreach (var l in _lines) o.Emit(l); }
    }
    private class ListSink : ISink<LogRecord>
    {
        public List<LogRecord> Items { get; } = new();
        public void Consume(LogRecord r) => Items.Add(r);
    }
    private class ListDlq : ISink<FailedRecord>
    {
        public List<FailedRecord> Items { get; } = new();
        public void Consume(FailedRecord f) => Items.Add(f);
    }
    private class FlakyStage : IStage<LogRecord, LogRecord>
    {
        private int _fails;
        public FlakyStage(int fails) => _fails = fails;
        public void Process(LogRecord r, IEmitter<LogRecord> o)
        {
            if (_fails-- > 0) throw new StageException("geçici hata");
            o.Emit(r);
        }
    }

    private static StageSlot Parse(ErrorPolicy p) => new(new ParserStage(), "1:parse", p);

    [Fact]
    public void Skip_CountsAndContinues()
    {
        var sink = new ListSink();
        var pl = new Pipeline(new ListSource(Good, Bad, Good),
            new List<StageSlot> { Parse(new ErrorPolicy(ErrorPolicyKind.Skip)) }, sink);
        var s = pl.Run();

        Assert.Equal(2, sink.Items.Count);
        Assert.Equal(1, s.Skipped);
        Assert.True(s.IsBalanced);
    }

    [Fact]
    public void DeadLetter_RoutesFailedRecordWithContext()
    {
        var sink = new ListSink(); var dlq = new ListDlq();
        var pl = new Pipeline(new ListSource(Good, Bad),
            new List<StageSlot> { Parse(new ErrorPolicy(ErrorPolicyKind.DeadLetter)) }, sink, dlq);
        var s = pl.Run();

        Assert.Single(sink.Items);
        Assert.Single(dlq.Items);
        Assert.Equal("garbage", dlq.Items[0].RawInput);
        Assert.Equal("1:parse", dlq.Items[0].StageName);
        Assert.IsType<StageException>(dlq.Items[0].Exception);
        Assert.Equal(1, s.DeadLettered);
        Assert.True(s.IsBalanced);
    }

    [Fact]
    public void FailFast_ThrowsPipelineException()
    {
        var pl = new Pipeline(new ListSource(Good, Bad, Good),
            new List<StageSlot> { Parse(ErrorPolicy.Default) }, new ListSink());
        Assert.Throws<PipelineException>(() => pl.Run());
    }

    [Fact]
    public void DeadLetterPolicyWithoutChannel_ThrowsConfigException()
    {
        Assert.Throws<ConfigException>(() => new Pipeline(new ListSource(),
            new List<StageSlot> { Parse(new ErrorPolicy(ErrorPolicyKind.DeadLetter)) }, new ListSink()));
    }

    [Fact]
    public void Retry_SucceedsAfterTransientFailures()
    {
        var sink = new ListSink();
        var slots = new List<StageSlot>
        {
            Parse(ErrorPolicy.Default),
            new StageSlot(new FlakyStage(2), "2:flaky", new ErrorPolicy(ErrorPolicyKind.Retry, 3)),
        };
        var s = new Pipeline(new ListSource(Good), slots, sink).Run();

        Assert.Single(sink.Items);
        Assert.Equal(2, s.Stages[1].Retried);
    }

    [Fact]
    public void EmptySource_IsBalancedWithZeroCounts()
    {
        var s = new Pipeline(new ListSource(),
            new List<StageSlot> { Parse(ErrorPolicy.Default) }, new ListSink()).Run();
        Assert.Equal(0, s.Incoming);
        Assert.True(s.IsBalanced);
    }

    [Theory]
    [InlineData("SKIP", ErrorPolicyKind.Skip)]
    [InlineData("dead_letter", ErrorPolicyKind.DeadLetter)]
    [InlineData("RETRY(3)", ErrorPolicyKind.Retry)]
    [InlineData(null, ErrorPolicyKind.FailFast)]
    public void ErrorPolicy_Parses(string? text, ErrorPolicyKind kind) =>
        Assert.Equal(kind, ErrorPolicy.Parse(text, "stage #1").Kind);

    [Fact]
    public void ErrorPolicy_InvalidValue_Throws() =>
        Assert.Throws<ConfigException>(() => ErrorPolicy.Parse("IGNORE", "stage #1"));
}