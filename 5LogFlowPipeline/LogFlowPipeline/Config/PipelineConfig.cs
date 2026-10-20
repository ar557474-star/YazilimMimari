namespace LogFlowPipeline.Config;

using System.Collections.Generic;

public class PipelineConfig
{
    public PipelineInfo? pipeline { get; set; }
    public ComponentConfig? source { get; set; }
    public List<ComponentConfig>? stages { get; set; }
    public ComponentConfig? sink { get; set; }
}

public class PipelineInfo { public string? name { get; set; } }

public class ComponentConfig
{
    public string? type { get; set; }
    public Dictionary<string, object>? parameters { get; set; }
}