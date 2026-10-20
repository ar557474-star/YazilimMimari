namespace LogFlowPipeline.Tests;

using Xunit;
using LogFlowPipeline;

public class PipelineLoaderTests
{
    private static PipelineLoader NewLoader() =>
        new(ComponentRegistry.Discover(typeof(ParserStage).Assembly));

    private static string TempLog()
    {
        var p = Path.GetTempFileName();
        File.WriteAllText(p, "");
        return p.Replace('\\', '/');
    }

    private static string Yaml(string src, string stages) => $@"
pipeline:
  name: test
  source:
    type: file
    params: {{ path: '{src}' }}
  stages:
{stages}
  sink:
    type: file
    params: {{ path: 'out/test.txt' }}
";

    [Fact]
    public void ValidConfig_BuildsPipeline()
    {
        var y = Yaml(TempLog(), @"    - type: parse
      params: { format: clf }
    - type: filter
      params: { field: status, op: "">="", value: 400 }");
        var pipeline = NewLoader().LoadFromYaml(y);
        Assert.Equal("test", pipeline.Name);
    }

    [Fact]
    public void UnknownStageType_Throws()
    {
        var y = Yaml(TempLog(), "    - type: filtr\n      params: {}");
        var ex = Assert.Throws<ConfigException>(() => NewLoader().LoadFromYaml(y));
        Assert.Contains("filtr", ex.Message);
        Assert.Contains("filter", ex.Message); // kullanılabilir türleri listeler
    }

    [Fact]
    public void MissingParameter_Throws()
    {
        var y = Yaml(TempLog(), @"    - type: parse
      params: { format: clf }
    - type: filter
      params: { field: status, value: 400 }");
        var ex = Assert.Throws<ConfigException>(() => NewLoader().LoadFromYaml(y));
        Assert.Contains("'op'", ex.Message);
    }

    [Fact]
    public void WrongParameterType_Throws()
    {
        var y = Yaml(TempLog(), @"    - type: parse
      params: { format: clf }
    - type: filter
      params: { field: status, op: "">="", value: abc }");
        var ex = Assert.Throws<ConfigException>(() => NewLoader().LoadFromYaml(y));
        Assert.Contains("value", ex.Message);
    }

    [Fact]
    public void EmptyPipeline_Throws()
    {
        var y = Yaml(TempLog(), "    []");
        Assert.Throws<ConfigException>(() => NewLoader().LoadFromYaml(y));
    }
}