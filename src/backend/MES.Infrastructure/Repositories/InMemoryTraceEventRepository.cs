using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemoryTraceEventRepository : ITraceEventRepository
{
    private readonly ConcurrentDictionary<string, List<TraceEvent>> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(TraceEvent traceEvent, CancellationToken cancellationToken = default)
    {
        var list = _store.GetOrAdd(traceEvent.Sn, _ => new List<TraceEvent>());
        lock (list)
        {
            list.Add(traceEvent);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TraceEvent>> GetBySnAsync(string sn, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(sn, out var list))
        {
            return Task.FromResult<IReadOnlyList<TraceEvent>>(Array.Empty<TraceEvent>());
        }

        lock (list)
        {
            return Task.FromResult<IReadOnlyList<TraceEvent>>(list.ToList());
        }
    }
}
