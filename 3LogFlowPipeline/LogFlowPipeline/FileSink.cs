namespace LogFlowPipeline;

using System.IO;

public class FileSink : ISink<LogRecord>
{
    private readonly string _outputPath;
    private StreamWriter? _writer;

    public FileSink(string outputPath)
    {
        _outputPath = outputPath;
    }

    public void Open()
    {
        _writer = new StreamWriter(_outputPath, false);
    }

    public void Close()
    {
        _writer?.Dispose();
    }

    public void Consume(LogRecord record)
    {
        _writer?.WriteLine($"[{record.Timestamp:yyyy-MM-dd HH:mm:ss}] {record.ClientIp} | {record.Method} {record.Path} -> {record.Status}");
    }
}