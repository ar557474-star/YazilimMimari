namespace LogFlowPipeline;

using LogFlowPipeline.Config;
using System;
using System.Collections.Generic;

[Stage("filter")]
public class ConfigurableFilterStage : IStage<LogRecord, LogRecord>
{
    private readonly string _field;
    private readonly string _op;
    private readonly string _value;
    private readonly Predicate<LogRecord> _predicate;

    public ConfigurableFilterStage(IDictionary<string, object> parameters)
    {
        _field = parameters["field"].ToString()!;
        _op = parameters["op"].ToString()!;
        _value = parameters["value"].ToString()!;

        _predicate = field.ToLower() switch
        {
            "status" => op switch
            {
                ">=" => r => r.Status >= int.Parse(_value),
                "<=" => r => r.Status <= int.Parse(_value),
                "==" => r => r.Status == int.Parse(_value),
                ">" => r => r.Status > int.Parse(_value),
                "<" => r => r.Status < int.Parse(_value),
                _ => throw new ArgumentException($"Bilinmeyen operatör: '{op}'")
            },
            "path" => op switch
            {
                "startsWith" => r => r.Path.StartsWith(_value),
                "==" => r => r.Path == _value,
                _ => throw new ArgumentException($"Bilinmeyen operatör: '{op}'")
            },
            "clientip" => r => r.ClientIp == _value,
            _ => throw new ArgumentException($"Bilinmeyen alan: '{field}'")
        };
    }

    public void Process(LogRecord input, IEmitter<LogRecord> output)
    {
        if (_predicate(input)) output.Emit(input);
    }

    public void Open() { }
    public void Close() { }
}