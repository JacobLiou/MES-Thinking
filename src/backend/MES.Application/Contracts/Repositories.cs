using MES.Domain.Entities;

namespace MES.Application.Contracts;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByNoAsync(string workOrderNo, CancellationToken cancellationToken = default);
    Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkOrder workOrder, CancellationToken cancellationToken = default);
}

public interface ISerialUnitRepository
{
    Task<SerialUnit?> GetBySnAsync(string sn, CancellationToken cancellationToken = default);
    Task AddAsync(SerialUnit serialUnit, CancellationToken cancellationToken = default);
    Task UpdateAsync(SerialUnit serialUnit, CancellationToken cancellationToken = default);
}

public interface ITestRecordRepository
{
    Task<bool> ExistsAsync(string sn, string stationCode, string testBatchId, CancellationToken cancellationToken = default);
    Task AddAsync(TestRecord testRecord, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestRecord>> GetBySnAsync(string sn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestRecord>> GetByTimeRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? stationCode,
        CancellationToken cancellationToken = default);
}

public interface ITraceEventRepository
{
    Task AddAsync(TraceEvent traceEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TraceEvent>> GetBySnAsync(string sn, CancellationToken cancellationToken = default);
}

public interface IStationRepository
{
    Task<Station?> GetByCodeAsync(string stationCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Station>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Station station, CancellationToken cancellationToken = default);
}

public interface ITestFlowRepository
{
    Task<TestFlow?> GetByCodeAsync(string flowCode, CancellationToken cancellationToken = default);
    Task<TestFlow?> GetActiveByProductCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestFlow>> GetAllAsync(string? productCode, CancellationToken cancellationToken = default);
    Task AddAsync(TestFlow testFlow, CancellationToken cancellationToken = default);
    Task UpdateAsync(TestFlow testFlow, CancellationToken cancellationToken = default);
    Task DeactivateByProductCodeAsync(string productCode, string exceptFlowCode, CancellationToken cancellationToken = default);
}

public interface ISpcRuleRepository
{
    Task<SpcRule?> GetByCodeAsync(string ruleCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpcRule>> GetAllAsync(string? productCode, string? stationCode, CancellationToken cancellationToken = default);
    Task AddAsync(SpcRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(SpcRule rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(string ruleCode, CancellationToken cancellationToken = default);
}

public interface IAlarmEventRepository
{
    Task AddAsync(AlarmEvent alarmEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlarmEvent>> GetLatestAsync(int count, string? stationCode, CancellationToken cancellationToken = default);
}
