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
}

public interface ITraceEventRepository
{
    Task AddAsync(TraceEvent traceEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TraceEvent>> GetBySnAsync(string sn, CancellationToken cancellationToken = default);
}
