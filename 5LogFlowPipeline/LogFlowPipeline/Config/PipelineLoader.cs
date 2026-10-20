namespace LogFlowPipeline.Config;

using System;
using System.IO;
using YamlDotNet.Serialization;

public static class PipelineLoader
{
    public static Pipeline Load(string configPath)
    {
        // 1. OKU
        if (!File.Exists(configPath))
            throw new ConfigException($"Yapılandırma dosyası bulunamadı: {configPath}");

        var yaml = File.ReadAllText(configPath);
        var config = new DeserializerBuilder().Build()
                        .Deserialize<PipelineConfig>(yaml)
                     ?? throw new ConfigException("Boş veya geçersiz yapılandırma dosyası.");

        // 2. DOĞRULA (veri işlenmeden ÖNCE - fail fast)
        if (config.source == null || string.IsNullOrWhiteSpace(config.source.type))
            throw new ConfigException("Eksik bölüm: 'source' ve source.type zorunludur.");
        if (config.sink == null || string.IsNullOrWhiteSpace(config.sink.type))
            throw new ConfigException("Eksik bölüm: 'sink' ve sink.type zorunludur.");
        if (config.stages == null || config.stages.Count == 0)
            throw new ConfigException("Boş işlem hattı: en az bir stage tanımlanmalı.");

        foreach (var s in config.stages)
        {
            var tip = StageRegistry.Find(s.type ?? "");
            if (tip == null)
                throw new ConfigException($"Bilinmeyen aşama tipi: '{s.type}'. " +
                    $"Kayıtlı tipler: {string.Join(", ", StageRegistry.RegisteredNames)}");
            try { StageFactory.CreateStage(tip, s.parameters ?? new()); }  // param tür kontrolü burada
            catch (Exception ex) { throw new ConfigException($"Aşama '{s.type}': {ex.Message}"); }
        }

        // 3. ÜRET
        var source = (ISource<string>)StageFactory.CreateSource(config.source);
        var builder = PipelineBuilder.From(source);
        foreach (var s in config.stages)
            builder = builder.Via(StageFactory.CreateStage(s.type!, s.parameters ?? new()));
        return builder.To((ISink<LogRecord>)StageFactory.CreateSink(config.sink));
    }
}

public class ConfigException : Exception
{
    public ConfigException(string msg) : base(msg) { }
}