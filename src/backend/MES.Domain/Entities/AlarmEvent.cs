namespace MES.Domain.Entities;

public sealed class AlarmEvent
{
    public required string AlarmCode { get; init; }
    public required string Sn { get; init; }
    public required string StationCode { get; init; }
    public required string MetricName { get; init; }
    public double MetricValue { get; init; }
    public double? LowerLimit { get; init; }
    public double? UpperLimit { get; init; }
    public required string Severity { get; init; }
    public required string Status { get; init; }
    public required string Message { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
