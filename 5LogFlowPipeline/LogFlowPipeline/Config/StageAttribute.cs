namespace LogFlowPipeline.Config;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class StageAttribute : Attribute
{
    public string Name { get; }
    public StageAttribute(string name) => Name = name;
}