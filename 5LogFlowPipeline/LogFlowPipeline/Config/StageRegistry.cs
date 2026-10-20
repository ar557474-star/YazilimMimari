namespace LogFlowPipeline.Config;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class StageRegistry
{
    // "parse" -> typeof(ParserStage)  gibi eşlemeler burada tutulur
    private static readonly Dictionary<string, Type> _registry = new();

    public static void Scan(Assembly assembly)
    {
        var tipler = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new { Tip = t, Attr = t.GetCustomAttribute<StageAttribute>() })
            .Where(x => x.Attr != null);

        foreach (var x in tipler)
            _registry[x.Attr!.Name] = x.Tip;   // son eklenen kazanır
    }

    public static Type? Find(string name) =>
        _registry.TryGetValue(name, out var t) ? t : null;

    public static IEnumerable<string> RegisteredNames => _registry.Keys;
}