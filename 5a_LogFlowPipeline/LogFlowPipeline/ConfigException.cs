namespace LogFlowPipeline;

/// Yapılandırma zamanı hataları (çalışma zamanı veri hatalarından ayrı tutulur).
public class ConfigException : Exception
{
    public ConfigException(string message) : base(message) { }
}