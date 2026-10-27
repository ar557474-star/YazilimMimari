namespace LogFlowPipeline;

/// Hat devam edemez (config, okunamayan dosya, disk dolu). Kurtarılamaz.
public class PipelineException : Exception
{
    public PipelineException(string message) : base(message) { }
    public PipelineException(string message, Exception inner) : base(message, inner) { }
}

/// Tek bir kayıt bir aşamada işlenemedi. Kurtarılabilir: politika karar verir.
public class StageException : Exception
{
    public StageException(string message) : base(message) { }
    public StageException(string message, Exception inner) : base(message, inner) { }
}

/// Yapılandırma zamanı hatası. Hat hiç başlamaz. PipelineException'ın alt türüdür.
public class ConfigException : PipelineException
{
    public ConfigException(string message) : base(message) { }
}