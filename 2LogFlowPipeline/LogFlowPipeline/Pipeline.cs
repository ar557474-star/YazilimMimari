/*public class Pipeline
{
    private readonly ISource<string> _source;
    private readonly ISink<string> _sink;

    public Pipeline(ISource<string> source, ISink<string> sink)
    {
        _source = source;
        _sink = sink;
    }

    public void Run()
    {
        // Source veriyi ürettikçe doğrudan Sink'e aktaran minik köprü (DirectEmitter)
        _source.Produce(new DirectEmitter(_sink));
    }

    private class DirectEmitter : IEmitter<string>
    {
        private readonly ISink<string> _sink;

        public DirectEmitter(ISink<string> sink)
        {
            _sink = sink;
        }

        public void Emit(string item)
        {
            _sink.Consume(item);
        }
    }
}*/namespace LogFlowPipeline;

public class Pipeline
{
    private readonly ISource<string> _source;
    private readonly IStage<string, LogRecord> _stage;
    private readonly ISink<LogRecord> _sink;

    // 3 Parametreli Constructor (Hatanın çözümü)
    public Pipeline(ISource<string> source, IStage<string, LogRecord> stage, ISink<LogRecord> sink)
    {
        _source = source;
        _stage = stage;
        _sink = sink;
    }

    public void Run()
    {
        _stage.Open();
        _source.Produce(new StageEmitter(_stage, _sink));
        _stage.Close();
    }

    private class StageEmitter : IEmitter<string>
    {
        private readonly IStage<string, LogRecord> _stage;
        private readonly ISink<LogRecord> _sink;

        public StageEmitter(IStage<string, LogRecord> stage, ISink<LogRecord> sink)
        {
            _stage = stage;
            _sink = sink;
        }

        public void Emit(string item)
        {
            _stage.Process(item, new SinkEmitter(_sink));
        }
    }

    private class SinkEmitter : IEmitter<LogRecord>
    {
        private readonly ISink<LogRecord> _sink;

        public SinkEmitter(ISink<LogRecord> sink) => _sink = sink;

        public void Emit(LogRecord item) => _sink.Consume(item);
    }
}