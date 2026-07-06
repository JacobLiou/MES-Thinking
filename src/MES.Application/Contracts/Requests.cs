namespace MES.Application.Contracts;

public sealed class CreateWorkOrderRequest
{
    public required string WorkOrderNo { get; init; }
    public required string ProductCode { get; init; }
    public int PlannedQty { get; init; }
}

public sealed class StationPassRequest
{
    public required string Sn { get; init; }
    public required string WorkOrderNo { get; init; }
    public required string StationCode { get; init; }
    public required string OperatorId { get; init; }
}

public sealed class UploadTestResultRequest
{
    public required string Sn { get; init; }
    public required string StationCode { get; init; }
    public required string TestBatchId { get; init; }
    public bool Passed { get; init; }
    public Dictionary<string, double>? Metrics { get; init; }
    public string? RawPayload { get; init; }
}

public sealed class CreateStationRequest
{
    public required string StationCode { get; init; }
    public required string Name { get; init; }
    public required string LineCode { get; init; }
    public bool IsTestStation { get; init; }
}

public sealed class CreateRouteStepRequest
{
    public int Sequence { get; init; }
    public required string StepCode { get; init; }
    public required string StationCode { get; init; }
    public string StepType { get; init; } = "PassOnly";
    public bool AllowRework { get; init; }
}

public sealed class CreateTestFlowRequest
{
    public required string FlowCode { get; init; }
    public required string Name { get; init; }
    public required string ProductCode { get; init; }
    public required string Version { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<CreateRouteStepRequest> Steps { get; init; } = [];
}
