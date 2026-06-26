using MES.Domain.Enums;

namespace MES.Domain.Entities;

public sealed class SerialUnit
{
    public required string Sn { get; init; }
    public required string WorkOrderNo { get; init; }
    public required string CurrentStationCode { get; set; }
    public SerialStatus Status { get; set; } = SerialStatus.Created;
    public bool? LastTestPassed { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
