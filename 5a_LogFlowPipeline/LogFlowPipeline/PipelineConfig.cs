namespace LogFlowPipeline;
//PipelineConfig.cs (YAML’ın C# karşılığı)
public class PipelineFile { public PipelineDef? Pipeline { get; set; } }

public class PipelineDef
{
    public string? Name { get; set; }
    public ComponentDef? Source { get; set; }
    public List<ComponentDef>? Stages { get; set; }
    public ComponentDef? Sink { get; set; }
}

public class ComponentDef
{
    public string? Type { get; set; }
    public Dictionary<string, object>? Params { get; set; }
}