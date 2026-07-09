using MES.Application.Contracts;

namespace MES.Application.Services;

public interface ISpcService
{
    Task<SpcSummaryResponse> GetSummaryAsync(
        string? productCode,
        string? stationCode,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpcRuleResponse>> GetRulesAsync(
        string? productCode,
        string? stationCode,
        CancellationToken cancellationToken = default);

    Task<CommandResult> CreateRuleAsync(
        CreateSpcRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<CommandResult> UpdateRuleAsync(
        string ruleCode,
        UpdateSpcRuleRequest request,
        CancellationToken cancellationToken = default);

    Task<CommandResult> DeleteRuleAsync(
        string ruleCode,
        CancellationToken cancellationToken = default);

    Task<DashboardRealtimeResponse> GetRealtimeAsync(
        string? productCode,
        string? stationCode,
        CancellationToken cancellationToken = default);
}
