/*namespace LogFlowPipeline;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class ParserStage : IStage<string, LogRecord>
{
    public int ErrorCount { get; private set; } = 0;

    //private static readonly Regex ClfRegex = new Regex(
    //    @"^(\S+)\s+\S+\s+\S+\s+\[([^\]]+)\]\s+""(\S+)\s+(\S+)\s+[^""]*""\s+(\d{3})\s+(\S+)(?:\s+""([^""]*)""\s+""([^""]*)""") ? ",
    //    RegexOptions.Compiled);
    private static readonly Regex ClfRegex = new Regex(
    @"^(\S+)\s+\S+\s+\S+\s+\[([^\]]+)\]\s+""(\S+)\s+(\S+)\s+[^""]*""\s+(\d{3})\s+(\S+)(?:\s+""([^""]*)""\s+""([^""]*)"")?",
    RegexOptions.Compiled);

    public void Open() { }

    public void Close() { }

    public void Process(string input, IEmitter<LogRecord> output)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            ErrorCount++;
            return;
        }

        var match = ClfRegex.Match(input);
        if (!match.Success)
        {
            ErrorCount++;
            return;
        }

        try
        {
            string ip = match.Groups[1].Value;
            string dateStr = match.Groups[2].Value;
            string method = match.Groups[3].Value;
            string path = match.Groups[4].Value;
            int status = int.Parse(match.Groups[5].Value);

            string bytesStr = match.Groups[6].Value;
            long bytes = bytesStr == "-" ? 0 : long.Parse(bytesStr);
            string userAgent = match.Groups[8].Success ? match.Groups[8].Value : "-";

            DateTimeOffset timestamp = DateTimeOffset.ParseExact(
                dateStr,
                "dd/MMM/yyyy:HH:mm:ss zzz",
                CultureInfo.InvariantCulture);

            var record = new LogRecord(
                timestamp, ip, method, path, status, bytes, userAgent,
                new Dictionary<string, string>(), input
            );

            output.Emit(record);
        }
        catch
        {
            ErrorCount++;
        }
    }
}*/
using LogFlowPipeline;
using System.Globalization;
using System.Text.RegularExpressions;

public class CustomLogParserStage : IStage<string, LogRecord>
{
    public int ErrorCount { get; private set; } = 0;

    // 2026-10-13 10:15:20 192.168.1.10 GET /api/users 200 OK
    private static readonly Regex CustomRegex = new Regex(
        @"^(\d{4}-\d{2}-\d{2})\s+(\d{2}:\d{2}:\d{2})\s+(\S+)\s+(\S+)\s+(\S+)\s+(\d{3})\s+(.*)$",
        RegexOptions.Compiled);

    public void Process(string input, IEmitter<LogRecord> output)
    {
        if (string.IsNullOrWhiteSpace(input)) { ErrorCount++; return; }

        var match = CustomRegex.Match(input);
        if (!match.Success) { ErrorCount++; return; }

        try
        {
            var timestamp = DateTimeOffset.ParseExact(
                $"{match.Groups[1].Value} {match.Groups[2].Value}",
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture);

            var record = new LogRecord(
                timestamp,
                match.Groups[3].Value,              // IP
                match.Groups[4].Value,              // Method
                match.Groups[5].Value,              // Path
                int.Parse(match.Groups[6].Value),   // Status
                0, "-",                             // Bytes, UserAgent
                new Dictionary<string, string>(), input);

            output.Emit(record);
        }
        catch { ErrorCount++; }
    }
}