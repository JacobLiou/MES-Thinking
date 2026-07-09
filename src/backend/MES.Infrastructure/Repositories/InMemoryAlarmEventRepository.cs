using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemoryAlarmEventRepository : IAlarmEventRepository
{
    private readonly ConcurrentDictionary<string, AlarmEvent> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(AlarmEvent alarmEvent, CancellationToken cancellationToken = default)
    {
        _store[alarmEvent.AlarmCode] = alarmEvent;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AlarmEvent>> GetLatestAsync(
        int count,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var query = _store.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(stationCode))
        {
            query = query.Where(x => x.StationCode.Equals(stationCode, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<AlarmEvent>>(query
            .OrderByDescending(x => x.OccurredAt)
            .Take(count)
            .ToList());
    }
}
