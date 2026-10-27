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


/*
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
*/

namespace LogFlowPipeline;

using System.Diagnostics;

public class Pipeline
{
    private readonly object _source;
    private readonly List<StageSlot> _slots;
    private readonly object _sink;
    private readonly ISink<FailedRecord>? _deadLetter;

    public string Name { get; init; } = "unnamed";
    public RunSummary? LastSummary { get; private set; }

    public Pipeline(object source, List<StageSlot> slots, object sink, ISink<FailedRecord>? deadLetter = null)
    {
        _source = source; _slots = slots; _sink = sink; _deadLetter = deadLetter;

        if (deadLetter == null && slots.Any(s => s.Policy.Kind == ErrorPolicyKind.DeadLetter))
            throw new ConfigException(
                "Bir aşama DEAD_LETTER politikası kullanıyor ama pipeline.deadLetter tanımlı değil.");
    }

    // Eski kullanım (PipelineBuilder) bozulmasın: varsayılan politika FAIL_FAST
    public Pipeline(object source, List<object> stages, object sink)
        : this(source,
               stages.Select((s, i) => new StageSlot(s, $"{i + 1}:{s.GetType().Name}", ErrorPolicy.Default)).ToList(),
               sink)
    { }

    public RunSummary Run()
    {
        if (_source is not ISource<string> source)
            throw new PipelineException("Kaynak metin satırı (ISource<string>) üretmiyor.");
        if (_sink is not ISink<LogRecord> sink)
            throw new PipelineException("Hedef LogRecord kabul etmiyor.");

        foreach (var s in _slots) s.Stats = new StageStats(s.Name);
        var summary = new RunSummary(Name, _slots.Select(s => s.Stats).ToList());
        LastSummary = summary;   // hata olsa bile Program kısmi özeti yazdırabilsin

        var sw = Stopwatch.StartNew();
        sink.Open();
        _deadLetter?.Open();
        foreach (var s in _slots) s.Open();

        try
        {
            source.Produce(new ActionEmitter<string>(line => ProcessLine(line, sink, summary)));
        }
        finally
        {
            foreach (var s in _slots) s.Close();
            sink.Close();
            _deadLetter?.Close();
            sw.Stop();
            summary.Elapsed = sw.Elapsed;
        }
        return summary;
    }

    // Bir satırın hattaki yolculuğu
    private void ProcessLine(string line, ISink<LogRecord> sink, RunSummary summary)
    {
        summary.Incoming++;
        var current = new List<object> { line };

        foreach (var slot in _slots)
        {
            var next = new List<object>();
            foreach (var item in current)
                RunStage(slot, item, line, next);
            current = next;
            if (current.Count == 0) return;   // filtrelendi / atıldı / dead-letter'a gitti
        }

        foreach (var item in current)
        {
            if (item is not LogRecord rec) continue;
            try { sink.Consume(rec); }
            catch (IOException ex)
            {
                throw new PipelineException($"Hedefe yazılamadı: {ex.Message}", ex);   // disk dolu vb.
            }
            summary.Outgoing++;
        }
    }

    private void RunStage(StageSlot slot, object item, string raw, List<object> output)
    {
        var stats = slot.Stats;
        stats.In++;

        int maxAttempts = slot.Policy.Kind == ErrorPolicyKind.Retry ? slot.Policy.RetryCount + 1 : 1;
        int attempt = 0;

        while (true)
        {
            var buffer = new List<object>();   // yarım kalan çıktı sızmasın
            long t0 = Stopwatch.GetTimestamp();
            try
            {
                slot.Invoke(item, buffer);
                stats.Out += buffer.Count;
                output.AddRange(buffer);
                return;
            }
            catch (PipelineException) { throw; }   // kurtarılamaz: politika bile bakmaz
            catch (Exception ex)
            {
                var se = ex as StageException
                         ?? new StageException($"Beklenmeyen hata: {ex.Message}", ex);
                attempt++;
                if (attempt < maxAttempts) { stats.Retried++; continue; }
                HandleFailure(slot, raw, se);
                return;
            }
            finally
            {
                stats.Elapsed += Stopwatch.GetElapsedTime(t0);
            }
        }
    }

    // Politika uygulaması: hata yönlendirmesi topolojik karardır
    private void HandleFailure(StageSlot slot, string raw, StageException ex)
    {
        switch (slot.Policy.Kind)
        {
            case ErrorPolicyKind.Skip:
                slot.Stats.Skipped++;
                break;

            case ErrorPolicyKind.DeadLetter:
                slot.Stats.DeadLettered++;
                try
                {
                    _deadLetter!.Consume(new FailedRecord(raw, slot.Name, ex, DateTimeOffset.Now));
                }
                catch (IOException io)
                {
                    // Teşhis kanalı da yazılamıyorsa sessiz veri kaybı olur: hat durur
                    throw new PipelineException($"Dead-letter hedefine yazılamadı: {io.Message}", io);
                }
                break;

            default: // FAIL_FAST ve denemeleri tükenmiş RETRY
                throw new PipelineException(
                    $"'{slot.Name}' aşamasında hata (politika {slot.Policy}): {ex.Message}", ex);
        }
    }
}