namespace LogFlowPipeline;

using System.Collections.Generic;
using System.Linq;

public class ProjectionStage : IStage<LogRecord, LogRecord>
{
    private readonly string[] _fields;

    public ProjectionStage(params string[] fields)
    {
        _fields = fields;
    }

    public void Open() { }

    public void Close() { }

    public void Process(LogRecord input, IEmitter<LogRecord> output)
    {
        // İstenen alanları filtreleyip yeni bir sözlükte tutabiliriz
        var projectedAttributes = new Dictionary<string, string>();

        foreach (var field in _fields)
        {
            switch (field.ToLower())
            {
                case "timestamp": projectedAttributes[field] = input.Timestamp.ToString(); break;
                case "clientip": projectedAttributes[field] = input.ClientIp; break;
                case "method": projectedAttributes[field] = input.Method; break;
                case "path": projectedAttributes[field] = input.Path; break;
                case "status": projectedAttributes[field] = input.Status.ToString(); break;
            }
        }

        var projectedRecord = input with { Attributes = projectedAttributes };
        output.Emit(projectedRecord);
    }
}