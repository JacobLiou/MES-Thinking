using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemoryWorkOrderRepository : IWorkOrderRepository
{
    private readonly ConcurrentDictionary<string, WorkOrder> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<WorkOrder?> GetByNoAsync(string workOrderNo, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(workOrderNo, out var workOrder);
        return Task.FromResult(workOrder);
    }

    public Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        _store[workOrder.WorkOrderNo] = workOrder;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        _store[workOrder.WorkOrderNo] = workOrder;
        return Task.CompletedTask;
    }
}
