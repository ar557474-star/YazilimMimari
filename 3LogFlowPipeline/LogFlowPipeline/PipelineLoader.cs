namespace LogFlowPipeline;

using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class PipelineLoader
{
    private readonly ComponentRegistry _registry;

    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public PipelineLoader(ComponentRegistry registry) => _registry = registry;

    public Pipeline LoadFromFile(string path)
    {
        if (!File.Exists(path))
            throw new ConfigException($"Yapılandırma dosyası bulunamadı: {path}");
        return LoadFromYaml(File.ReadAllText(path));
    }

    public Pipeline LoadFromYaml(string yaml)
    {
        PipelineFile? file;
        try { file = Yaml.Deserialize<PipelineFile>(yaml); }
        catch (YamlException ex)
        {
            throw new ConfigException(
                $"YAML biçim hatası (satır {ex.Start.Line}): {ex.InnerException?.Message ?? ex.Message}");
        }

        var def = file?.Pipeline ?? throw new ConfigException("Kök düğüm 'pipeline:' bulunamadı.");
        if (def.Source == null) throw new ConfigException("'pipeline.source' eksik.");
        if (def.Sink == null) throw new ConfigException("'pipeline.sink' eksik.");
        if (def.Stages == null || def.Stages.Count == 0)
            throw new ConfigException("pipeline.stages boş; en az bir 'parse' aşaması gerekli.");

        // Hepsi Run()'dan ÖNCE oluşturulur -> hata varsa hiç veri işlenmez
        var source = _registry.Create(ComponentKind.Source, "source", def.Source);
        var stages = new List<object>();
        for (int i = 0; i < def.Stages.Count; i++)
            stages.Add(_registry.Create(ComponentKind.Stage, $"stage #{i + 1}", def.Stages[i]));
        var sink = _registry.Create(ComponentKind.Sink, "sink", def.Sink);

        // Tip zinciri kontrolü
        if (source is not ISource<string>)
            throw new ConfigException("source: bu bileşen metin satırı üretmiyor.");
        for (int i = 0; i < stages.Count; i++)
        {
            bool ok = i == 0 ? stages[i] is IStage<string, LogRecord>
                             : stages[i] is IStage<LogRecord, LogRecord>;
            if (!ok)
                throw new ConfigException(i == 0
                    ? "stage #1: ilk aşama 'parse' (metin → LogRecord) olmalı."
                    : $"stage #{i + 1}: LogRecord → LogRecord aşaması olmalı; 'parse' yalnızca ilk sırada kullanılabilir.");
        }
        if (sink is not ISink<LogRecord>)
            throw new ConfigException("sink: bu bileşen LogRecord kabul etmiyor.");

        return new Pipeline(source, stages, sink) { Name = def.Name ?? "unnamed" };
    }
}