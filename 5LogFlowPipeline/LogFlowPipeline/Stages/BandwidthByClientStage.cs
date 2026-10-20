namespace LogFlowPipeline;

using LogFlowPipeline.Config;
using System.Collections.Generic;

[Stage("bandwidth-by-client")]
public class BandwidthByClientStage : IStage<LogRecord, LogRecord>
{
    private readonly Dictionary<string, long> _toplam = new();
    private readonly string? _outputPath;

    public BandwidthByClientStage(IDictionary<string, object>? parameters = null)
    {
        _outputPath = parameters != null && parameters.TryGetValue("output", out var o)
                      ? o.ToString() : null;
    }

    public void Process(LogRecord input, IEmitter<LogRecord> output)
    {
        _toplam[input.ClientIp] = _toplam.GetValueOrDefault(input.ClientIp) + input.Bytes;
        // Bu stage tek tek kayıt geçirmez; sonucu Close()'da verir.
    }

    public void Open() { }

    public void Close()
    {
        // Basit çözüm: sonucu konsola veya kendi dosyasına yaz
        using var w = _outputPath != null ? new StreamWriter(_outputPath) : null;
        foreach (var (ip, bytes) in _toplam.OrderByDescending(x => x.Value))
        {
            var satir = $"{ip}\t{bytes} bytes";
            if (w != null) w.WriteLine(satir); else Console.WriteLine(satir);
        }
    }
}