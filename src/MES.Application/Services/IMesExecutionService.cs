using MES.Application.Contracts;

namespace MES.Application.Services;

public interface IMesExecutionService
{
    Task<CommandResult> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default);
    Task<CommandResult> PassStationAsync(StationPassRequest request, CancellationToken cancellationToken = default);
    Task<CommandResult> UploadTestResultAsync(UploadTestResultRequest request, CancellationToken cancellationToken = default);
    Task<TraceabilityResponse?> GetTraceabilityAsync(string sn, CancellationToken cancellationToken = default);
}
