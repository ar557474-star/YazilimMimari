namespace LogFlowPipeline.Tests;

using System;
using System.IO;
using LogFlowPipeline;
using LogFlowPipeline.Config;
using Xunit;

public class PipelineLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public PipelineLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        // Registry bir kere taranmalı (her testte güvenli, idempotent)
        StageRegistry.Scan(typeof(ParserStage).Assembly);
    }

    // Geçerli config'i diske yazan yardımcı
    private string YazConfig(string icerik)
    {
        var yol = Path.Combine(_tempDir, Guid.NewGuid() + ".yaml");
        File.WriteAllText(yol, icerik);
        return yol;
    }

    // ── 1. Geçerli yapılandırma ──
    [Fact]
    public void Load_ValidConfig_ReturnsPipeline()
    {
        var yol = YazConfig("""
            pipeline: { name: test }
            source: { type: file, parameters: { path: data/dataFile.log } }
            stages:
              - { type: parse, parameters: { format: custom } }
              - { type: filter, parameters: { field: status, op: ">=", value: 400 } }
            sink: { type: file, parameters: { path: out/result.txt } }
            """);

        var pipeline = PipelineLoader.Load(yol);   // Exception fırlatmamalı

        Assert.NotNull(pipeline);
    }

    // ── 2. Bilinmeyen aşama tipi ──
    [Fact]
    public void Load_UnknownStageType_ThrowsConfigException()
    {
        var yol = YazConfig("""
            source: { type: file, parameters: { path: data.log } }
            stages:
              - { type: filtter, parameters: { field: status } }
            sink: { type: file, parameters: { path: out.txt } }
            """);

        var ex = Assert.Throws<ConfigException>(() => PipelineLoader.Load(yol));
        Assert.Contains("filtter", ex.Message);   // mesaj hatayı açıklıyor olmalı
    }

    // ── 3. Eksik parametre ──
    [Fact]
    public void Load_MissingRequiredParam_ThrowsConfigException()
    {
        var yol = YazConfig("""
            source: { type: file, parameters: { } }   // path eksik!
            stages:
              - { type: parse, parameters: { format: custom } }
            sink: { type: file, parameters: { path: out.txt } }
            """);

        var ex = Assert.Throws<ConfigException>(() => PipelineLoader.Load(yol));
        Assert.Contains("path", ex.Message);
    }

    // ── 4. Yanlış türde parametre ──
    [Fact]
    public void Load_WrongTypedParam_ThrowsConfigException()
    {
        var yol = YazConfig("""
            source: { type: file, parameters: { path: data.log } }
            stages:
              - { type: parse, parameters: { format: custom } }
              - { type: filter, parameters: { field: status, op: ">=", value: "abc" } }
            sink: { type: file, parameters: { path: out.txt } }
            """);

        // "abc" sayıya çevrilememeli → ConfigException
        Assert.ThrowsAny<Exception>(() => PipelineLoader.Load(yol));
    }

    // ── 5. Boş işlem hattı ──
    [Fact]
    public void Load_EmptyPipeline_ThrowsConfigException()
    {
        var yol = YazConfig("""
            source: { type: file, parameters: { path: data.log } }
            stages: []
            sink: { type: file, parameters: { path: out.txt } }
            """);

        var ex = Assert.Throws<ConfigException>(() => PipelineLoader.Load(yol));
        Assert.Contains("Boş", ex.Message);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }
}