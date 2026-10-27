namespace LogFlowPipeline;

using System.Text;

[DeadLetter("file")]
public class DeadLetterFileSink : ISink<FailedRecord>
{
    private const int MaxRawChars = 200;   // 100 KiB'lık satırı dosyaya olduğu gibi yazmayalım
    private readonly string _path;
    private StreamWriter? _writer;

    public DeadLetterFileSink(ParamReader p) => _path = p.GetString("path");

    public void Open()
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        _writer = new StreamWriter(_path, false, new UTF8Encoding(false));
    }

    public void Close() => _writer?.Dispose();

    public void Consume(FailedRecord f)
    {
        string raw = f.RawInput.Replace('\r', ' ').Replace('\n', ' ');
        if (raw.Length > MaxRawChars)
            raw = raw[..MaxRawChars] + $"...(+{raw.Length - MaxRawChars} karakter)";

        _writer?.WriteLine(
            $"{f.Timestamp:yyyy-MM-dd HH:mm:ss} | stage={f.StageName} | " +
            $"{f.Exception.GetType().Name}: {f.Exception.Message} | raw={raw}");
    }
}