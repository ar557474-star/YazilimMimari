namespace LogFlowPipeline;

[AttributeUsage(AttributeTargets.Class)]
public abstract class ComponentAttribute : Attribute
{
    public string Name { get; }
    protected ComponentAttribute(string name) => Name = name;
}

public class SourceAttribute : ComponentAttribute { public SourceAttribute(string n) : base(n) { } }
public class StageAttribute : ComponentAttribute { public StageAttribute(string n) : base(n) { } }
public class SinkAttribute : ComponentAttribute { public SinkAttribute(string n) : base(n) { } }


public class DeadLetterAttribute : ComponentAttribute { public DeadLetterAttribute(string n) : base(n) { } }