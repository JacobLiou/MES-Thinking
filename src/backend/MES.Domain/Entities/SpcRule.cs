namespace MES.Domain.Entities;

public sealed class SpcRule
{
    public required string RuleCode { get; init; }
    public required string MetricName { get; init; }
    public string? ProductCode { get; init; }
    public string? StationCode { get; init; }
    public double? LowerLimit { get; init; }
    public double? UpperLimit { get; init; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
