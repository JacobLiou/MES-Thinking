using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemorySerialUnitRepository : ISerialUnitRepository
{
    private readonly ConcurrentDictionary<string, SerialUnit> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<SerialUnit?> GetBySnAsync(string sn, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(sn, out var serialUnit);
        return Task.FromResult(serialUnit);
    }

    public Task AddAsync(SerialUnit serialUnit, CancellationToken cancellationToken = default)
    {
        _store[serialUnit.Sn] = serialUnit;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SerialUnit serialUnit, CancellationToken cancellationToken = default)
    {
        _store[serialUnit.Sn] = serialUnit;
        return Task.CompletedTask;
    }
}
