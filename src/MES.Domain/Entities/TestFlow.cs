namespace MES.Domain.Entities;

public sealed class TestFlow
{
    public required string FlowCode { get; init; }
    public required string Name { get; init; }
    public required string ProductCode { get; init; }
    public required string Version { get; init; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<RouteStep> Steps { get; init; } = [];
}
