using MES.Application.Contracts;

namespace MES.Application.Services;

public interface IStationService
{
    Task<CommandResult> CreateStationAsync(CreateStationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StationResponse>> GetStationsAsync(CancellationToken cancellationToken = default);
}
