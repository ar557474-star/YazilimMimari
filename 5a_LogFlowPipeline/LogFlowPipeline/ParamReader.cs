namespace LogFlowPipeline;

public class ParamReader
{
    private readonly string _owner;
    private readonly IDictionary<string, object> _p;

    public ParamReader(string owner, IDictionary<string, object>? p)
    {
        _owner = owner;
        _p = p ?? new Dictionary<string, object>();
    }

    public ConfigException Fail(string message) => new($"{_owner}: {message}");

    private object Require(string key)
    {
        if (!_p.TryGetValue(key, out var v) || v is null || (v is string s && s.Trim() == ""))
            throw Fail($"zorunlu parametre '{key}' eksik.");
        return v;
    }

    public string GetString(string key)
    {
        var v = Require(key);
        if (v is not string s)
            throw Fail($"'{key}' parametresi metin olmalı.");
        return s;
    }

    public long GetLong(string key)
    {
        var v = Require(key);
        if (v is string s && long.TryParse(s, out var n)) return n;
        throw Fail($"'{key}' parametresi tamsayı olmalı, '{v}' bulundu.");
    }

    public List<string> GetStringList(string key)
    {
        var v = Require(key);
        if (v is not List<object> list)
            throw Fail($"'{key}' parametresi liste olmalı. Örnek: {key}: [a, b]");
        if (list.Count == 0)
            throw Fail($"'{key}' listesi boş olamaz.");
        return list.Select(x => x?.ToString() ?? "").ToList();
    }

    public string GetOneOf(string key, params string[] allowed)
    {
        var s = GetString(key);
        var hit = allowed.FirstOrDefault(a => a.Equals(s, StringComparison.OrdinalIgnoreCase));
        if (hit == null)
            throw Fail($"'{key}' için geçersiz değer '{s}'. Geçerli değerler: {string.Join(", ", allowed)}");
        return hit.ToLowerInvariant();
    }
}