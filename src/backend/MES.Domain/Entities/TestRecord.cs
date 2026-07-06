namespace MES.Domain.Entities;

public sealed class TestRecord
{
    public required string Sn { get; init; }
    public required string StationCode { get; init; }
    public required string TestBatchId { get; init; }
    public required bool Passed { get; init; }
    public Dictionary<string, double> Metrics { get; init; } = new();
    public string? RawPayload { get; init; }
    public DateTimeOffset TestedAt { get; init; } = DateTimeOffset.UtcNow;
}
