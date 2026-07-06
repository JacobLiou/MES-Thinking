namespace MES.Domain.Entities;

public sealed class Station
{
    public required string StationCode { get; init; }
    public required string Name { get; init; }
    public required string LineCode { get; init; }
    public bool IsTestStation { get; init; }
}
