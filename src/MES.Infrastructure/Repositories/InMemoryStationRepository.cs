using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemoryStationRepository : IStationRepository
{
    private readonly ConcurrentDictionary<string, Station> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<Station?> GetByCodeAsync(string stationCode, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(stationCode, out var station);
        return Task.FromResult(station);
    }

    public Task<IReadOnlyList<Station>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Station>>(_store.Values.OrderBy(s => s.StationCode).ToList());

    public Task AddAsync(Station station, CancellationToken cancellationToken = default)
    {
        if (_store.ContainsKey(station.StationCode))
        {
            throw new InvalidOperationException($"Station '{station.StationCode}' already exists.");
        }

        _store[station.StationCode] = station;
        return Task.CompletedTask;
    }
}
