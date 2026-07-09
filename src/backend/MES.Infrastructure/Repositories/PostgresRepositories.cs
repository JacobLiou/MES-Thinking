using Microsoft.EntityFrameworkCore;
using MES.Application.Contracts;
using MES.Domain.Entities;
using MES.Infrastructure.Persistence;

namespace MES.Infrastructure.Repositories;

public sealed class PostgresWorkOrderRepository : IWorkOrderRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresWorkOrderRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<WorkOrder?> GetByNoAsync(string workOrderNo, CancellationToken cancellationToken = default) =>
        _dbContext.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkOrderNo == workOrderNo, cancellationToken);

    public async Task AddAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkOrders.Add(workOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkOrder workOrder, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkOrders.Update(workOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class PostgresSerialUnitRepository : ISerialUnitRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresSerialUnitRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SerialUnit?> GetBySnAsync(string sn, CancellationToken cancellationToken = default) =>
        _dbContext.SerialUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Sn == sn, cancellationToken);

    public async Task AddAsync(SerialUnit serialUnit, CancellationToken cancellationToken = default)
    {
        _dbContext.SerialUnits.Add(serialUnit);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SerialUnit serialUnit, CancellationToken cancellationToken = default)
    {
        _dbContext.SerialUnits.Update(serialUnit);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class PostgresTestRecordRepository : ITestRecordRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresTestRecordRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(string sn, string stationCode, string testBatchId, CancellationToken cancellationToken = default) =>
        _dbContext.TestRecords.AnyAsync(
            x => x.Sn == sn && x.StationCode == stationCode && x.TestBatchId == testBatchId,
            cancellationToken);

    public async Task AddAsync(TestRecord testRecord, CancellationToken cancellationToken = default)
    {
        _dbContext.TestRecords.Add(testRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestRecord>> GetBySnAsync(string sn, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.TestRecords
            .AsNoTracking()
            .Where(x => x.Sn == sn)
            .OrderBy(x => x.TestedAt)
            .ToListAsync(cancellationToken);

        return records;
    }

    public async Task<IReadOnlyList<TestRecord>> GetByTimeRangeAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TestRecords
            .AsNoTracking()
            .Where(x => x.TestedAt >= from && x.TestedAt <= to);

        if (!string.IsNullOrWhiteSpace(stationCode))
        {
            query = query.Where(x => x.StationCode == stationCode);
        }

        return await query
            .OrderBy(x => x.TestedAt)
            .ToListAsync(cancellationToken);
    }
}

public sealed class PostgresTraceEventRepository : ITraceEventRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresTraceEventRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TraceEvent traceEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.TraceEvents.Add(traceEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TraceEvent>> GetBySnAsync(string sn, CancellationToken cancellationToken = default)
    {
        var events = await _dbContext.TraceEvents
            .AsNoTracking()
            .Where(x => x.Sn == sn)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync(cancellationToken);

        return events;
    }
}

public sealed class PostgresStationRepository : IStationRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresStationRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Station?> GetByCodeAsync(string stationCode, CancellationToken cancellationToken = default) =>
        _dbContext.Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StationCode == stationCode, cancellationToken);

    public async Task<IReadOnlyList<Station>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stations = await _dbContext.Stations
            .AsNoTracking()
            .OrderBy(x => x.StationCode)
            .ToListAsync(cancellationToken);

        return stations;
    }

    public async Task AddAsync(Station station, CancellationToken cancellationToken = default)
    {
        _dbContext.Stations.Add(station);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class PostgresTestFlowRepository : ITestFlowRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresTestFlowRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TestFlow?> GetByCodeAsync(string flowCode, CancellationToken cancellationToken = default)
    {
        var flow = await _dbContext.TestFlows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FlowCode == flowCode, cancellationToken);

        if (flow is null)
        {
            return null;
        }

        var steps = await _dbContext.RouteSteps
            .AsNoTracking()
            .Where(x => x.FlowCode == flow.FlowCode)
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        return ComposeFlow(flow, steps);
    }

    public async Task<TestFlow?> GetActiveByProductCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        var flow = await _dbContext.TestFlows
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive, cancellationToken);

        if (flow is null)
        {
            return null;
        }

        var steps = await _dbContext.RouteSteps
            .AsNoTracking()
            .Where(x => x.FlowCode == flow.FlowCode)
            .OrderBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        return ComposeFlow(flow, steps);
    }

    public async Task<IReadOnlyList<TestFlow>> GetAllAsync(string? productCode, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TestFlows.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(productCode))
        {
            query = query.Where(x => x.ProductCode == productCode);
        }

        var flows = await query
            .OrderBy(x => x.FlowCode)
            .ToListAsync(cancellationToken);

        if (flows.Count == 0)
        {
            return Array.Empty<TestFlow>();
        }

        var flowCodes = flows.Select(x => x.FlowCode).ToArray();
        var stepLookup = await _dbContext.RouteSteps
            .AsNoTracking()
            .Where(x => flowCodes.Contains(x.FlowCode))
            .OrderBy(x => x.FlowCode)
            .ThenBy(x => x.Sequence)
            .ToListAsync(cancellationToken);

        var groupedSteps = stepLookup
            .GroupBy(x => x.FlowCode)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<RouteStep>)x.ToList(), StringComparer.OrdinalIgnoreCase);

        return flows
            .Select(flow => ComposeFlow(flow, groupedSteps.GetValueOrDefault(flow.FlowCode, Array.Empty<RouteStep>())))
            .ToList();
    }

    public async Task AddAsync(TestFlow testFlow, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.TestFlows.Add(new TestFlow
        {
            FlowCode = testFlow.FlowCode,
            Name = testFlow.Name,
            ProductCode = testFlow.ProductCode,
            Version = testFlow.Version,
            IsActive = testFlow.IsActive,
            CreatedAt = testFlow.CreatedAt,
            Steps = []
        });

        foreach (var step in testFlow.Steps)
        {
            _dbContext.RouteSteps.Add(new RouteStep
            {
                FlowCode = step.FlowCode,
                Sequence = step.Sequence,
                StepCode = step.StepCode,
                StationCode = step.StationCode,
                StepType = step.StepType,
                AllowRework = step.AllowRework
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpdateAsync(TestFlow testFlow, CancellationToken cancellationToken = default)
    {
        _dbContext.TestFlows.Update(new TestFlow
        {
            FlowCode = testFlow.FlowCode,
            Name = testFlow.Name,
            ProductCode = testFlow.ProductCode,
            Version = testFlow.Version,
            IsActive = testFlow.IsActive,
            CreatedAt = testFlow.CreatedAt,
            Steps = []
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateByProductCodeAsync(
        string productCode,
        string exceptFlowCode,
        CancellationToken cancellationToken = default)
    {
        var flows = await _dbContext.TestFlows
            .Where(x => x.ProductCode == productCode && x.FlowCode != exceptFlowCode && x.IsActive)
            .ToListAsync(cancellationToken);

        if (flows.Count == 0)
        {
            return;
        }

        foreach (var flow in flows)
        {
            flow.IsActive = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TestFlow ComposeFlow(TestFlow flow, IReadOnlyList<RouteStep> steps) =>
        new()
        {
            FlowCode = flow.FlowCode,
            Name = flow.Name,
            ProductCode = flow.ProductCode,
            Version = flow.Version,
            IsActive = flow.IsActive,
            CreatedAt = flow.CreatedAt,
            Steps = steps
        };
}

public sealed class PostgresSpcRuleRepository : ISpcRuleRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresSpcRuleRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SpcRule?> GetByCodeAsync(string ruleCode, CancellationToken cancellationToken = default) =>
        _dbContext.SpcRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RuleCode == ruleCode, cancellationToken);

    public async Task<IReadOnlyList<SpcRule>> GetAllAsync(
        string? productCode,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SpcRules.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(productCode))
        {
            query = query.Where(x => x.ProductCode == productCode);
        }

        if (!string.IsNullOrWhiteSpace(stationCode))
        {
            query = query.Where(x => x.StationCode == stationCode);
        }

        return await query
            .OrderBy(x => x.RuleCode)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SpcRule rule, CancellationToken cancellationToken = default)
    {
        _dbContext.SpcRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SpcRule rule, CancellationToken cancellationToken = default)
    {
        _dbContext.SpcRules.Update(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string ruleCode, CancellationToken cancellationToken = default)
    {
        var rule = await _dbContext.SpcRules.FirstOrDefaultAsync(x => x.RuleCode == ruleCode, cancellationToken);
        if (rule is null)
        {
            return;
        }

        _dbContext.SpcRules.Remove(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class PostgresAlarmEventRepository : IAlarmEventRepository
{
    private readonly MesDbContext _dbContext;

    public PostgresAlarmEventRepository(MesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AlarmEvent alarmEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.AlarmEvents.Add(alarmEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AlarmEvent>> GetLatestAsync(
        int count,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AlarmEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(stationCode))
        {
            query = query.Where(x => x.StationCode == stationCode);
        }

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
