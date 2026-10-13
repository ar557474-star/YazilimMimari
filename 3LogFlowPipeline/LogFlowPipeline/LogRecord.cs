namespace LogFlowPipeline;

using System;
using System.Collections.Generic;

public record LogRecord(
    DateTimeOffset Timestamp,
    string ClientIp,
    string Method,
    string Path,
    int Status,
    long Bytes,
    string UserAgent,
    Dictionary<string, string> Attributes,
    string Raw
);