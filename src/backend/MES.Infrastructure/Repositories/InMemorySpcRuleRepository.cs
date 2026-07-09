using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemorySpcRuleRepository : ISpcRuleRepository
{
    private readonly ConcurrentDictionary<string, SpcRule> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<SpcRule?> GetByCodeAsync(string ruleCode, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(ruleCode, out var rule);
        return Task.FromResult(rule);
    }

    public Task<IReadOnlyList<SpcRule>> GetAllAsync(
        string? productCode,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var query = _store.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(productCode))
        {
            query = query.Where(x => string.Equals(x.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(stationCode))
        {
            query = query.Where(x => string.Equals(x.StationCode, stationCode, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<SpcRule>>(query.OrderBy(x => x.RuleCode).ToList());
    }

    public Task AddAsync(SpcRule rule, CancellationToken cancellationToken = default)
    {
        _store[rule.RuleCode] = rule;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SpcRule rule, CancellationToken cancellationToken = default)
    {
        _store[rule.RuleCode] = rule;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string ruleCode, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(ruleCode, out _);
        return Task.CompletedTask;
    }
}
