namespace LogFlowPipeline.Config;

using System;
using System.Collections.Generic;

public static class StageFactory
{
    public static object CreateStage(string type, IDictionary<string, object> parameters)
    {
        var tip = StageRegistry.Find(type)
            ?? throw new ConfigException($"Bilinmeyen aşama tipi: '{type}'");

        // Tek parametreli constructor (IDictionary<string,object>) aranır
        try
        {
            return Activator.CreateInstance(tip, parameters)!;
        }
        catch (MissingMethodException)
        {
            return Activator.CreateInstance(tip)!;  // parametresiz stage'ler
        }
    }

    // Source/Sink için de aynı mantık (type: "file" iki kez kayıtlı,
    // ISource mi ISink mi istediğimizi hedef arayüze bakarak ayırt ederiz)
    public static ISource<string> CreateSource(ComponentConfig c) =>
        (ISource<string>)Instantiate(c, typeof(ISource<string>));
    public static ISink<LogRecord> CreateSink(ComponentConfig c) =>
        (ISink<LogRecord>)Instantiate(c, typeof(ISink<LogRecord>));

    private static object Instantiate(ComponentConfig c, Type arayuz)
    {
        var tip = StageRegistry.Find(c.type ?? "")
            ?? throw new ConfigException($"Bilinmeyen tip: '{c.type}'");
        if (!arayuz.IsAssignableFrom(tip))
            throw new ConfigException($"'{c.type}' bu pozisyonda kullanılamaz ({arayuz.Name} değil).");
        Require(c.parameters, "path");
        try { return Activator.CreateInstance(tip, c.parameters!)!; }
        catch { return Activator.CreateInstance(tip, c.parameters!["path"]!.ToString())!; }
    }

    private static void Require(Dictionary<string, object>? p, string key)
        => (p == null || !p.ContainsKey(key) || p[key] == null
            ? throw new ConfigException($"Eksik parametre: '{key}'") : (object)null!);
}