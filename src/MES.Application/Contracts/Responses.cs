namespace MES.Application.Contracts;

public sealed class CommandResult
{
    public required bool Success { get; init; }
    public required string Code { get; init; }
    public required string Message { get; init; }
}

public sealed class TraceabilityResponse
{
    public required string Sn { get; init; }
    public string? WorkOrderNo { get; init; }
    public string? CurrentStationCode { get; init; }
    public string? CurrentStatus { get; init; }
    public IReadOnlyList<TraceTimelineItem> Timeline { get; init; } = [];
    public IReadOnlyList<TestResultItem> TestResults { get; init; } = [];
}

public sealed class TraceTimelineItem
{
    public required string EventType { get; init; }
    public required string StationCode { get; init; }
    public required string OperatorId { get; init; }
    public string? Message { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

public sealed class TestResultItem
{
    public required string StationCode { get; init; }
    public required string TestBatchId { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyDictionary<string, double> Metrics { get; init; } = new Dictionary<string, double>();
    public DateTimeOffset TestedAt { get; init; }
}
