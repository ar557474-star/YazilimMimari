namespace LogFlowPipeline;

[Stage("filter")]
public class FilterStage : IStage<LogRecord, LogRecord>
{
    private static readonly string[] NumericFields = { "status", "bytes" };
    private static readonly string[] StringFields = { "clientip", "method", "path", "useragent" };
    private static readonly string[] NumericOps = { "==", "!=", ">", ">=", "<", "<=" };
    private static readonly string[] StringOps = { "==", "!=", "startswith", "contains" };

    private readonly Func<LogRecord, bool> _predicate;

    public FilterStage(ParamReader p)
    {
        string field = p.GetString("field").ToLowerInvariant();
        string op = p.GetString("op").ToLowerInvariant();

        if (NumericFields.Contains(field))
        {
            if (!NumericOps.Contains(op))
                throw p.Fail($"'{field}' alanı için geçersiz op '{op}'. Geçerli: {string.Join(" ", NumericOps)}");
            long value = p.GetLong("value");
            Func<LogRecord, long> get = field == "status" ? r => r.Status : r => r.Bytes;
            _predicate = op switch
            {
                "==" => r => get(r) == value,
                "!=" => r => get(r) != value,
                ">" => r => get(r) > value,
                ">=" => r => get(r) >= value,
                "<" => r => get(r) < value,
                _ => r => get(r) <= value,
            };
        }
        else if (StringFields.Contains(field))
        {
            if (!StringOps.Contains(op))
                throw p.Fail($"'{field}' alanı için geçersiz op '{op}'. Geçerli: {string.Join(" ", StringOps)}");
            string value = p.GetString("value");
            Func<LogRecord, string> get = field switch
            {
                "clientip" => r => r.ClientIp,
                "method" => r => r.Method,
                "path" => r => r.Path,
                _ => r => r.UserAgent,
            };
            _predicate = op switch
            {
                "==" => r => get(r) == value,
                "!=" => r => get(r) != value,
                "startswith" => r => get(r).StartsWith(value, StringComparison.Ordinal),
                _ => r => get(r).Contains(value, StringComparison.Ordinal),
            };
        }
        else
        {
            throw p.Fail($"bilinmeyen alan '{field}'. Geçerli: {string.Join(", ", NumericFields.Concat(StringFields))}");
        }
    }

    public void Open() { }
    public void Close() { }

    public void Process(LogRecord input, IEmitter<LogRecord> output)
    {
        if (_predicate(input)) output.Emit(input);
    }
}