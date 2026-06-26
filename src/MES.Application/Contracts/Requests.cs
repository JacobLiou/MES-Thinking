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
