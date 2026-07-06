using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemoryTestFlowRepository : ITestFlowRepository
{
    private readonly ConcurrentDictionary<string, TestFlow> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<TestFlow?> GetByCodeAsync(string flowCode, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(flowCode, out var flow);
        return Task.FromResult(flow);
    }

    public Task<TestFlow?> GetActiveByProductCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        var flow = _store.Values
            .FirstOrDefault(f =>
                f.IsActive
                && string.Equals(f.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(flow);
    }

    public Task<IReadOnlyList<TestFlow>> GetAllAsync(string? productCode, CancellationToken cancellationToken = default)
    {
        var flows = _store.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(productCode))
        {
            flows = flows.Where(f =>
                string.Equals(f.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<TestFlow>>(flows.OrderBy(f => f.FlowCode).ToList());
    }

    public Task AddAsync(TestFlow testFlow, CancellationToken cancellationToken = default)
    {
        if (_store.ContainsKey(testFlow.FlowCode))
        {
            throw new InvalidOperationException($"Test flow '{testFlow.FlowCode}' already exists.");
        }

        _store[testFlow.FlowCode] = testFlow;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TestFlow testFlow, CancellationToken cancellationToken = default)
    {
        _store[testFlow.FlowCode] = testFlow;
        return Task.CompletedTask;
    }

    public Task DeactivateByProductCodeAsync(
        string productCode,
        string exceptFlowCode,
        CancellationToken cancellationToken = default)
    {
        foreach (var flow in _store.Values)
        {
            if (string.Equals(flow.ProductCode, productCode, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(flow.FlowCode, exceptFlowCode, StringComparison.OrdinalIgnoreCase)
                && flow.IsActive)
            {
                flow.IsActive = false;
                _store[flow.FlowCode] = flow;
            }
        }

        return Task.CompletedTask;
    }
}
