using System.Collections.Concurrent;
using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Infrastructure.Repositories;

public sealed class InMemoryTestRecordRepository : ITestRecordRepository
{
    private readonly ConcurrentDictionary<string, TestRecord> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<bool> ExistsAsync(string sn, string stationCode, string testBatchId, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(sn, stationCode, testBatchId);
        return Task.FromResult(_store.ContainsKey(key));
    }

    public Task AddAsync(TestRecord testRecord, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(testRecord.Sn, testRecord.StationCode, testRecord.TestBatchId);
        _store[key] = testRecord;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TestRecord>> GetBySnAsync(string sn, CancellationToken cancellationToken = default)
    {
        var data = _store.Values.Where(x => x.Sn.Equals(sn, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IReadOnlyList<TestRecord>>(data);
    }

    private static string BuildKey(string sn, string stationCode, string testBatchId) =>
        $"{sn}|{stationCode}|{testBatchId}";
}
