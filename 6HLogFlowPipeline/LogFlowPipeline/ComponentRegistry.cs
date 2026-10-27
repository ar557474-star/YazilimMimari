/*namespace LogFlowPipeline;

using System.Reflection;

public enum ComponentKind { Source, Stage, Sink }

public class ComponentRegistry
{
    private readonly Dictionary<ComponentKind, Dictionary<string, Type>> _map = new()
    {
        [ComponentKind.Source] = new(StringComparer.OrdinalIgnoreCase),
        [ComponentKind.Stage] = new(StringComparer.OrdinalIgnoreCase),
        [ComponentKind.Sink] = new(StringComparer.OrdinalIgnoreCase),
    };

    public static ComponentRegistry Discover(Assembly assembly)
    {
        var r = new ComponentRegistry();
        foreach (var t in assembly.GetTypes())
        {
            if (t.IsAbstract) continue;
            var src = t.GetCustomAttribute<SourceAttribute>();
            var stg = t.GetCustomAttribute<StageAttribute>();
            var snk = t.GetCustomAttribute<SinkAttribute>();
            if (src != null) r.Add(ComponentKind.Source, src.Name, t);
            if (stg != null) r.Add(ComponentKind.Stage, stg.Name, t);
            if (snk != null) r.Add(ComponentKind.Sink, snk.Name, t);
        }
        return r;
    }

    private void Add(ComponentKind kind, string name, Type t)
    {
        var m = _map[kind];
        if (m.TryGetValue(name, out var existing))
            throw new InvalidOperationException(
                $"{kind} adı '{name}' iki sınıfta tanımlı: {existing.Name}, {t.Name}");
        m[name] = t;
    }

    /// where: hata mesajında görünecek konum, örn. "stage #2"
    public object Create(ComponentKind kind, string where, ComponentDef? def)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Type))
            throw new ConfigException($"{where}: 'type' alanı zorunludur.");

        var m = _map[kind];
        if (!m.TryGetValue(def.Type, out var type))
            throw new ConfigException(
                $"{where}: bilinmeyen tür '{def.Type}'. Kullanılabilir türler: {string.Join(", ", m.Keys.OrderBy(k => k))}");

        var reader = new ParamReader($"{where} ('{def.Type}')", def.Params);
        try
        {
            return Activator.CreateInstance(type, reader)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is ConfigException ce)
        {
            throw ce; // kurucudaki doğrulama hatasını olduğu gibi yüzeye çıkar
        }
    }
}*/

namespace LogFlowPipeline;

using System.Reflection;

public enum ComponentKind { Source, Stage, Sink, DeadLetter }

public class ComponentRegistry
{
    private readonly Dictionary<ComponentKind, Dictionary<string, Type>> _map = new()
    {
        [ComponentKind.Source] = new(StringComparer.OrdinalIgnoreCase),
        [ComponentKind.Stage] = new(StringComparer.OrdinalIgnoreCase),
        [ComponentKind.Sink] = new(StringComparer.OrdinalIgnoreCase),
        [ComponentKind.DeadLetter] = new(StringComparer.OrdinalIgnoreCase),
    };

    public static ComponentRegistry Discover(Assembly assembly)
    {
        var r = new ComponentRegistry();
        foreach (var t in assembly.GetTypes())
        {
            if (t.IsAbstract) continue;
            var src = t.GetCustomAttribute<SourceAttribute>();
            var stg = t.GetCustomAttribute<StageAttribute>();
            var snk = t.GetCustomAttribute<SinkAttribute>();
            var dlq = t.GetCustomAttribute<DeadLetterAttribute>();
            if (src != null) r.Add(ComponentKind.Source, src.Name, t);
            if (stg != null) r.Add(ComponentKind.Stage, stg.Name, t);
            if (snk != null) r.Add(ComponentKind.Sink, snk.Name, t);
            if (dlq != null) r.Add(ComponentKind.DeadLetter, dlq.Name, t);
        }
        return r;
    }

    private void Add(ComponentKind kind, string name, Type t)
    {
        var m = _map[kind];
        if (m.TryGetValue(name, out var existing))
            throw new InvalidOperationException(
                $"{kind} adı '{name}' iki sınıfta tanımlı: {existing.Name}, {t.Name}");
        m[name] = t;
    }

    public object Create(ComponentKind kind, string where, ComponentDef? def)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Type))
            throw new ConfigException($"{where}: 'type' alanı zorunludur.");

        var m = _map[kind];
        if (!m.TryGetValue(def.Type, out var type))
            throw new ConfigException(
                $"{where}: bilinmeyen tür '{def.Type}'. Kullanılabilir türler: {string.Join(", ", m.Keys.OrderBy(k => k))}");

        var reader = new ParamReader($"{where} ('{def.Type}')", def.Params);
        try
        {
            return Activator.CreateInstance(type, reader)!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is ConfigException ce)
        {
            throw ce;
        }
    }
}