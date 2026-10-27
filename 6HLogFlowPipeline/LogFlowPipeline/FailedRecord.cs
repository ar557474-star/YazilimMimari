namespace LogFlowPipeline;

public record FailedRecord(
    string RawInput,
    string StageName,
    Exception Exception,
    DateTimeOffset Timestamp);