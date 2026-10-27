namespace LogFlowPipeline;

public class StageStats
{
    public string Name { get; }
    public long In { get; set; }
    public long Out { get; set; }
    public long Skipped { get; set; }
    public long DeadLettered { get; set; }
    public long Retried { get; set; }
    public TimeSpan Elapsed { get; set; }

    // Hata yok ama emit etmedi = filtre tarafından elendi
    public long Filtered => In - Out - Skipped - DeadLettered;

    public StageStats(string name) => Name = name;
}

public class RunSummary
{
    public string PipelineName { get; }
    public List<StageStats> Stages { get; }
    public long Incoming { get; set; }
    public long Outgoing { get; set; }
    public TimeSpan Elapsed { get; set; }

    public RunSummary(string name, List<StageStats> stages)
    {
        PipelineName = name;
        Stages = stages;
    }

    public long Skipped => Stages.Sum(s => s.Skipped);
    public long DeadLettered => Stages.Sum(s => s.DeadLettered);
    public long Filtered => Stages.Sum(s => s.Filtered);

    /// Gelen = Çıkan + Filtrelenen + Atlanan + DeadLetter
    public bool IsBalanced => Incoming == Outgoing + Filtered + Skipped + DeadLettered;

    public void Print(TextWriter w)
    {
        w.WriteLine();
        w.WriteLine($"=== ÇALIŞTIRMA ÖZETİ: {PipelineName} ===");
        w.WriteLine($"Gelen kayıt         : {Incoming}");
        w.WriteLine($"Çıkan kayıt         : {Outgoing}");
        w.WriteLine($"Filtrelenen         : {Filtered}");
        w.WriteLine($"Atlanan (SKIP)      : {Skipped}");
        w.WriteLine($"Dead-letter         : {DeadLettered}");
        w.WriteLine($"Geçen süre          : {Elapsed}");
        w.WriteLine();
        w.WriteLine($"{"Aşama",-18}{"Giren",7}{"Çıkan",7}{"Filtre",8}{"Skip",6}{"DLQ",6}{"Retry",7}");
        foreach (var s in Stages)
            w.WriteLine($"{s.Name,-18}{s.In,7}{s.Out,7}{s.Filtered,8}{s.Skipped,6}{s.DeadLettered,6}{s.Retried,7}");
        w.WriteLine();
        w.WriteLine(IsBalanced
            ? $"Tutarlılık: {Incoming} = {Outgoing} + {Filtered} + {Skipped} + {DeadLettered}  OK"
            : $"Tutarlılık: HATA! {Incoming} != {Outgoing} + {Filtered} + {Skipped} + {DeadLettered}");
    }
}