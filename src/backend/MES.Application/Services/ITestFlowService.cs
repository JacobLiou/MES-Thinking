using MES.Application.Contracts;

namespace MES.Application.Services;

public interface ITestFlowService
{
    Task<CommandResult> CreateTestFlowAsync(CreateTestFlowRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestFlowResponse>> GetTestFlowsAsync(string? productCode, CancellationToken cancellationToken = default);
    Task<TestFlowResponse?> GetTestFlowByCodeAsync(string flowCode, CancellationToken cancellationToken = default);
    Task<CommandResult> ActivateTestFlowAsync(string flowCode, CancellationToken cancellationToken = default);
}
