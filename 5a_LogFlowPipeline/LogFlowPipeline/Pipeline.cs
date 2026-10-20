/*public class Pipeline
{
    private readonly ISource<string> _source;
    private readonly ISink<string> _sink;

    public Pipeline(ISource<string> source, ISink<string> sink)
    {
        _source = source;
        _sink = sink;
    }

    public void Run()
    {
        // Source veriyi ürettikçe doğrudan Sink'e aktaran minik köprü (DirectEmitter)
        _source.Produce(new DirectEmitter(_sink));
    }

    private class DirectEmitter : IEmitter<string>
    {
        private readonly ISink<string> _sink;

        public DirectEmitter(ISink<string> sink)
        {
            _sink = sink;
        }

        public void Emit(string item)
        {
            _sink.Consume(item);
        }
    }
}*/
/*namespace LogFlowPipeline;

public class Pipeline
{
    private readonly ISource<string> _source;
    private readonly IStage<string, LogRecord> _stage;
    private readonly ISink<LogRecord> _sink;

    // 3 Parametreli Constructor (Hatanın çözümü)
    public Pipeline(ISource<string> source, IStage<string, LogRecord> stage, ISink<LogRecord> sink)
    {
        _source = source;
        _stage = stage;
        _sink = sink;
    }

    public void Run()
    {
        _stage.Open();
        _source.Produce(new StageEmitter(_stage, _sink));
        _stage.Close();
    }

    private class StageEmitter : IEmitter<string>
    {
        private readonly IStage<string, LogRecord> _stage;
        private readonly ISink<LogRecord> _sink;

        public StageEmitter(IStage<string, LogRecord> stage, ISink<LogRecord> sink)
        {
            _stage = stage;
            _sink = sink;
        }

        public void Emit(string item)
        {
            _stage.Process(item, new SinkEmitter(_sink));
        }
    }

    private class SinkEmitter : IEmitter<LogRecord>
    {
        private readonly ISink<LogRecord> _sink;

        public SinkEmitter(ISink<LogRecord> sink) => _sink = sink;

        public void Emit(LogRecord item) => _sink.Consume(item);
    }
}*/
namespace LogFlowPipeline;

using System.Collections.Generic;

public class Pipeline
{
    private readonly object _source;
    private readonly List<object> _stages;
    private readonly object _sink;
    public string Name { get; init; } = "unnamed";

    // Builder'dan veya doğrudan çağrılabilen constructor
    public Pipeline(object source, List<object> stages, object sink)
    {
        _source = source;
        _stages = stages;
        _sink = sink;
    }

    public void Run()
    {
        // 1. Sink ve Stage'lerin Open metodlarını tetikle (Yaşam döngüsü)
        if (_sink is ISink<LogRecord> logSink) logSink.Open();

        foreach (var stage in _stages)
        {
            if (stage is IStage<string, LogRecord> s1) s1.Open();
            if (stage is IStage<LogRecord, LogRecord> s2) s2.Open();
        }

        // 2. Akışı başlat (Source -> Stage 1 -> Stage 2 -> ... -> Sink)
        // Burada veriyi zincirleme akıtacak bir mekanizma kuruyoruz.
        ExecutePipeline();

        // 3. Kapanış (Close) metotlarını tetikle
        foreach (var stage in _stages)
        {
            if (stage is IStage<string, LogRecord> s1) s1.Close();
            if (stage is IStage<LogRecord, LogRecord> s2) s2.Close();
        }
        if (_sink is ISink<LogRecord> logSinkClose) logSinkClose.Close();
    }

    private void ExecutePipeline()
    {
        // Basit ve esnek bir zincirleme akış simülasyonu:
        // Source string üretir -> ParserStage LogRecord üretir -> Filter/Projection -> Sink tüketir.

        if (_source is ISource<string> stringSource && _sink is ISink<LogRecord> finalSink)
        {
            // Eğer zincirde sadece ParserStage varsa veya Predicate/Projection varsa:
            stringSource.Produce(new ChainEmitter(_stages, finalSink));
        }
    }

    // Zincirdeki veriyi aşamadan aşamaya taşıyan yardımcı Emitter
    private class ChainEmitter : IEmitter<string>
    {
        private readonly List<object> _stages;
        private readonly ISink<LogRecord> _finalSink;

        public ChainEmitter(List<object> stages, ISink<LogRecord> finalSink)
        {
            _stages = stages;
            _finalSink = finalSink;
        }

        public void Emit(string line)
        {
            object currentData = line;

            foreach (var stage in _stages)
            {
                if (currentData == null) break;

                if (stage is IStage<string, LogRecord> parser && currentData is string str)
                {
                    var collector = new SingleItemCollector<LogRecord>();
                    parser.Process(str, collector);
                    currentData = collector.Item; // Bir sonraki aşamaya LogRecord geçer
                }
                else if (stage is IStage<LogRecord, LogRecord> filterOrProj && currentData is LogRecord rec)
                {
                    var collector = new SingleItemCollector<LogRecord>();
                    filterOrProj.Process(rec, collector);
                    currentData = collector.Item;
                }
            }

            if (currentData is LogRecord finalRecord)
            {
                _finalSink.Consume(finalRecord);
            }
        }
    }

    // Tek bir öğeyi geçici olarak yakalayan iç test/yardımcı sınıf
    private class SingleItemCollector<T> : IEmitter<T>
    {
        public T? Item { get; private set; }
        public void Emit(T item) => Item = item;
    }
}