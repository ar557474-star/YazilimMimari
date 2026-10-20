namespace LogFlowPipeline;

using System;

public class PredicateFilterStage : IStage<LogRecord, LogRecord>
{
    private readonly Predicate<LogRecord> _predicate;

    public PredicateFilterStage(Predicate<LogRecord> predicate)
    {
        _predicate = predicate;
    }

    public void Open() { }

    public void Close() { }

    public void Process(LogRecord input, IEmitter<LogRecord> output)
    {
        if (_predicate(input))
        {
            output.Emit(input);
        }
    }
}