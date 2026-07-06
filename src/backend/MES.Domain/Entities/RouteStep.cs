using MES.Domain.Enums;

namespace MES.Domain.Entities;

public sealed class RouteStep
{
    public required string FlowCode { get; init; }
    public int Sequence { get; init; }
    public required string StepCode { get; init; }
    public required string StationCode { get; init; }
    public StepType StepType { get; init; } = StepType.PassOnly;
    public bool AllowRework { get; init; }
}
