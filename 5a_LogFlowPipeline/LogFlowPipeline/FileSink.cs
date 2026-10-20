/*namespace LogFlowPipeline;

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
}*/


namespace LogFlowPipeline;

using System.IO;
using System.Linq;

[Sink("file")]
public class FileSink : ISink<LogRecord>
{
    private readonly string _outputPath;
    private StreamWriter? _writer;

    public FileSink(string outputPath) { _outputPath = outputPath; }
    public FileSink(ParamReader p) : this(p.GetString("path")) { }

    public void Open()
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(_outputPath));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        _writer = new StreamWriter(_outputPath, false);
    }

    public void Close() => _writer?.Dispose();

    public void Consume(LogRecord record)
    {
        if (record.Attributes.Count > 0)
            _writer?.WriteLine(string.Join(" | ", record.Attributes.Select(kv => $"{kv.Key}={kv.Value}")));
        else
            _writer?.WriteLine($"[{record.Timestamp:yyyy-MM-dd HH:mm:ss}] {record.ClientIp} | {record.Method} {record.Path} -> {record.Status}");
    }
}
