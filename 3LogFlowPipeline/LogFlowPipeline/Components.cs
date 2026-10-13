namespace LogFlowPipeline;

using System;
using System.IO;

// 1. Dosyadan oku ve satır satır Emitter'a ilet
public class FileLineSource : ISource<string>
{
    private readonly string _filePath;

    public FileLineSource(string filePath)
    {
        _filePath = filePath;
    }

    public void Produce(IEmitter<string> output)
    {
        if (!File.Exists(_filePath))
        {
            Console.WriteLine($"Hata: Dosya bulunamadı -> {_filePath}");
            return;
        }

        foreach (var line in File.ReadLines(_filePath))
        {
            output.Emit(line);
        }
    }
}

// 2. Kendisine gelen veriyi konsola yazdır
/*public class ConsoleSink : ISink<string>
{
    public void Consume(string item)
    {
        Console.WriteLine(item);
    }
}*/
public class ConsoleSink : ISink<LogRecord>
{
    public void Consume(LogRecord record)
    {
        Console.WriteLine($"[{record.Timestamp:yyyy-MM-dd HH:mm:ss}] IP: {record.ClientIp} | {record.Method} {record.Path} | Status: {record.Status} ({record.Bytes} bytes)");
    }
}