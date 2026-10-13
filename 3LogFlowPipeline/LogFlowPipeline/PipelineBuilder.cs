namespace LogFlowPipeline;

using System.Collections.Generic;

public class PipelineBuilder
{
    private object? _source;
    private readonly List<object> _stages = new();
    private object? _sink;

    public static PipelineBuilder From<T>(ISource<T> source)
    {
        return new PipelineBuilder { _source = source };
    }

    public PipelineBuilder Via<TIn, TOut>(IStage<TIn, TOut> stage)
    {
        _stages.Add(stage);
        return this;
    }

    public Pipeline To<T>(ISink<T> sink)
    {
        _sink = sink;
        // Burada source, stages ve sink tiplerini bağlayan dinamik bir Pipeline nesnesi döndüreceğiz.
        return new Pipeline(_source, _stages, _sink);
    }
}