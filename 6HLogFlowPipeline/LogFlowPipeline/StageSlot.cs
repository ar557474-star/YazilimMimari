namespace LogFlowPipeline;

public class StageSlot
{
    public object Stage { get; }
    public string Name { get; }
    public ErrorPolicy Policy { get; }
    public StageStats Stats { get; set; }

    public StageSlot(object stage, string name, ErrorPolicy policy)
    {
        Stage = stage; Name = name; Policy = policy;
        Stats = new StageStats(name);
    }

    public void Open()
    {
        if (Stage is IStage<string, LogRecord> a) a.Open();
        else if (Stage is IStage<LogRecord, LogRecord> b) b.Open();
    }

    public void Close()
    {
        if (Stage is IStage<string, LogRecord> a) a.Close();
        else if (Stage is IStage<LogRecord, LogRecord> b) b.Close();
    }

    /// Aşamayı çalıştırır, ürettiği çıktıları output listesine ekler.
    public void Invoke(object item, List<object> output)
    {
        switch (Stage)
        {
            case IStage<string, LogRecord> p when item is string s:
                p.Process(s, new ListEmitter<LogRecord>(output)); break;
            case IStage<LogRecord, LogRecord> f when item is LogRecord r:
                f.Process(r, new ListEmitter<LogRecord>(output)); break;
            default:
                throw new PipelineException(
                    $"'{Name}' aşaması {item.GetType().Name} türündeki girdiyi işleyemez.");
        }
    }
}

public class ListEmitter<T> : IEmitter<T>
{
    private readonly List<object> _list;
    public ListEmitter(List<object> list) => _list = list;
    public void Emit(T item) => _list.Add(item!);
}

public class ActionEmitter<T> : IEmitter<T>
{
    private readonly Action<T> _action;
    public ActionEmitter(Action<T> action) => _action = action;
    public void Emit(T item) => _action(item);
}