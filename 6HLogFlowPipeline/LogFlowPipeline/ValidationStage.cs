namespace LogFlowPipeline;

using System.Net;

[Stage("validate")]
public class ValidationStage : IStage<LogRecord, LogRecord>
{
    private static readonly string[] Allowed = { "clientip", "method", "path", "useragent" };

    private readonly List<string> _required;
    private readonly long _statusMin, _statusMax, _bytesMin;
    private readonly int _minYear, _maxYear;
    private readonly bool _checkIp;

    public ValidationStage(ParamReader p)
    {
        _required = p.GetStringListOr("requiredFields").Select(x => x.ToLowerInvariant()).ToList();
        foreach (var f in _required)
            if (!Allowed.Contains(f))
                throw p.Fail($"requiredFields içinde bilinmeyen alan '{f}'. Geçerli: {string.Join(", ", Allowed)}");

        _statusMin = p.GetLongOr("statusMin", 100);
        _statusMax = p.GetLongOr("statusMax", 599);
        _bytesMin = p.GetLongOr("bytesMin", 0);
        _minYear = (int)p.GetLongOr("minYear", 1990);
        _maxYear = (int)p.GetLongOr("maxYear", DateTime.UtcNow.Year + 1);
        _checkIp = p.GetBoolOr("validateIp", true);

        if (_statusMin > _statusMax) throw p.Fail("statusMin, statusMax'tan büyük olamaz.");
        if (_minYear > _maxYear) throw p.Fail("minYear, maxYear'dan büyük olamaz.");
    }

    public void Open() { }
    public void Close() { }

    public void Process(LogRecord r, IEmitter<LogRecord> output)
    {
        foreach (var f in _required)
        {
            string? v = f switch
            {
                "clientip" => r.ClientIp,
                "method" => r.Method,
                "path" => r.Path,
                _ => r.UserAgent,
            };
            if (string.IsNullOrWhiteSpace(v))
                throw new StageException($"Zorunlu alan '{f}' boş.");
        }

        if (r.Status < _statusMin || r.Status > _statusMax)
            throw new StageException($"status {r.Status} aralık dışı [{_statusMin}-{_statusMax}].");

        if (r.Bytes < _bytesMin)
            throw new StageException($"bytes {r.Bytes} en az {_bytesMin} olmalı.");

        int year = r.Timestamp.Year;
        if (year < _minYear || year > _maxYear)
            throw new StageException($"zaman damgası yılı {year} aralık dışı [{_minYear}-{_maxYear}].");

        if (_checkIp && !IsValidIp(r.ClientIp))
            throw new StageException($"geçersiz IP biçimi '{r.ClientIp}'.");

        output.Emit(r);
    }

    private static bool IsValidIp(string ip) =>
        IPAddress.TryParse(ip, out _) && (ip.Contains('.') || ip.Contains(':'));
}