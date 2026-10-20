namespace LogFlowPipeline.Tests;

using Xunit; // veya hangi test kütüphanesini kullanıyorsan
using LogFlowPipeline; // Ana projedeki sınıfları görebilmesi için ŞART

public class ParserStageTests
{
    [Fact]
    public void Process_ValidLine_EmitsRecord()
    {
        var parser = new ParserStage();
        var emitter = new CollectingEmitter<LogRecord>();
        string line = "127.0.0.1 - - [10/Oct/2000:13:55:36 -0700] \"GET /index.html HTTP/1.0\" 200 2326";

        parser.Process(line, emitter);

        Assert.Single(emitter.Items);
        Assert.Equal("127.0.0.1", emitter.Items[0].ClientIp);
        Assert.Equal(0, parser.ErrorCount);
    }

    [Fact]
    public void Process_EmptyLine_IncrementsErrorCount()
    {
        var parser = new ParserStage();
        var emitter = new CollectingEmitter<LogRecord>();

        parser.Process("", emitter);

        Assert.Empty(emitter.Items);
        Assert.Equal(1, parser.ErrorCount);
    }

    // Diğer testler: Hatalı zaman damgası, hatalı status kodu, eksik alan, fazladan boşluk vb.
}