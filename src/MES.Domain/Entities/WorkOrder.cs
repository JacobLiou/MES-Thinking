using MES.Domain.Enums;

namespace MES.Domain.Entities;

public sealed class WorkOrder
{
    public required string WorkOrderNo { get; init; }
    public required string ProductCode { get; init; }
    public int PlannedQty { get; init; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
    public string? TestFlowCode { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
