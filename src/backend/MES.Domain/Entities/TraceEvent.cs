namespace MES.Domain.Entities;

public sealed class TraceEvent
{
    public required string Sn { get; init; }
    public required string EventType { get; init; }
    public required string StationCode { get; init; }
    public required string OperatorId { get; init; }
    public string? Message { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
