using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Application.Services;

public sealed class SpcService : ISpcService
{
    private readonly ITestRecordRepository _testRecordRepository;
    private readonly ISerialUnitRepository _serialUnitRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ISpcRuleRepository _spcRuleRepository;
    private readonly IAlarmEventRepository _alarmEventRepository;

    public SpcService(
        ITestRecordRepository testRecordRepository,
        ISerialUnitRepository serialUnitRepository,
        IWorkOrderRepository workOrderRepository,
        ISpcRuleRepository spcRuleRepository,
        IAlarmEventRepository alarmEventRepository)
    {
        _testRecordRepository = testRecordRepository;
        _serialUnitRepository = serialUnitRepository;
        _workOrderRepository = workOrderRepository;
        _spcRuleRepository = spcRuleRepository;
        _alarmEventRepository = alarmEventRepository;
    }

    public async Task<SpcSummaryResponse> GetSummaryAsync(
        string? productCode,
        string? stationCode,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var windowEnd = to ?? DateTimeOffset.UtcNow;
        var windowStart = from ?? windowEnd.AddHours(-24);

        var records = await _testRecordRepository.GetByTimeRangeAsync(windowStart, windowEnd, stationCode, cancellationToken);
        var filtered = await FilterByProductAsync(records, productCode, cancellationToken);

        var sampleCount = filtered.Count;
        var passCount = filtered.Count(x => x.Passed);
        var failCount = sampleCount - passCount;

        var metricValues = filtered
            .SelectMany(x => x.Metrics.Select(metric => new { metric.Key, metric.Value }))
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new SpcMetricSummaryItem
            {
                MetricName = group.Key,
                Count = group.Count(),
                Mean = group.Average(x => x.Value),
                Min = group.Min(x => x.Value),
                Max = group.Max(x => x.Value)
            })
            .OrderBy(x => x.MetricName)
            .ToList();

        var firstPassBySn = filtered
            .GroupBy(x => x.Sn, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(x => x.TestedAt).First())
            .ToList();

        var fpy = firstPassBySn.Count == 0
            ? 0d
            : firstPassBySn.Count(x => x.Passed) / (double)firstPassBySn.Count;

        return new SpcSummaryResponse
        {
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            ProductCode = productCode,
            StationCode = stationCode,
            SampleCount = sampleCount,
            PassCount = passCount,
            FailCount = failCount,
            YieldRate = sampleCount == 0 ? 0d : passCount / (double)sampleCount,
            FirstPassYieldRate = fpy,
            Metrics = metricValues
        };
    }

    public async Task<IReadOnlyList<SpcRuleResponse>> GetRulesAsync(
        string? productCode,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var rules = await _spcRuleRepository.GetAllAsync(productCode, stationCode, cancellationToken);
        return rules
            .OrderBy(x => x.RuleCode)
            .Select(MapRule)
            .ToList();
    }

    public async Task<CommandResult> CreateRuleAsync(
        CreateSpcRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RuleCode) || string.IsNullOrWhiteSpace(request.MetricName))
        {
            return Failed("MES-4001", "SPC rule request is invalid.");
        }

        if (!request.LowerLimit.HasValue && !request.UpperLimit.HasValue)
        {
            return Failed("MES-4001", "At least one SPC limit is required.");
        }

        if (request.LowerLimit.HasValue && request.UpperLimit.HasValue && request.LowerLimit > request.UpperLimit)
        {
            return Failed("MES-4001", "LowerLimit cannot be greater than UpperLimit.");
        }

        var existing = await _spcRuleRepository.GetByCodeAsync(request.RuleCode, cancellationToken);
        if (existing is not null)
        {
            return Failed("MES-4091", "SPC rule already exists.");
        }

        await _spcRuleRepository.AddAsync(new SpcRule
        {
            RuleCode = request.RuleCode.Trim(),
            MetricName = request.MetricName.Trim(),
            ProductCode = request.ProductCode?.Trim(),
            StationCode = request.StationCode?.Trim(),
            LowerLimit = request.LowerLimit,
            UpperLimit = request.UpperLimit,
            IsActive = request.IsActive
        }, cancellationToken);

        return Success("SPC rule created.");
    }

    public async Task<CommandResult> UpdateRuleAsync(
        string ruleCode,
        UpdateSpcRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ruleCode) || string.IsNullOrWhiteSpace(request.MetricName))
        {
            return Failed("MES-4001", "SPC rule update request is invalid.");
        }

        var existing = await _spcRuleRepository.GetByCodeAsync(ruleCode, cancellationToken);
        if (existing is null)
        {
            return Failed("MES-4002", "SPC rule does not exist.");
        }

        if (request.LowerLimit.HasValue && request.UpperLimit.HasValue && request.LowerLimit > request.UpperLimit)
        {
            return Failed("MES-4001", "LowerLimit cannot be greater than UpperLimit.");
        }

        await _spcRuleRepository.UpdateAsync(new SpcRule
        {
            RuleCode = existing.RuleCode,
            MetricName = request.MetricName.Trim(),
            ProductCode = request.ProductCode?.Trim(),
            StationCode = request.StationCode?.Trim(),
            LowerLimit = request.LowerLimit,
            UpperLimit = request.UpperLimit,
            IsActive = request.IsActive,
            CreatedAt = existing.CreatedAt
        }, cancellationToken);

        return Success("SPC rule updated.");
    }

    public async Task<CommandResult> DeleteRuleAsync(string ruleCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ruleCode))
        {
            return Failed("MES-4001", "Rule code is invalid.");
        }

        var existing = await _spcRuleRepository.GetByCodeAsync(ruleCode, cancellationToken);
        if (existing is null)
        {
            return Failed("MES-4002", "SPC rule does not exist.");
        }

        await _spcRuleRepository.DeleteAsync(ruleCode, cancellationToken);
        return Success("SPC rule deleted.");
    }

    public async Task<DashboardRealtimeResponse> GetRealtimeAsync(
        string? productCode,
        string? stationCode,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(productCode, stationCode, DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow, cancellationToken);
        var alarms = await _alarmEventRepository.GetLatestAsync(20, stationCode, cancellationToken);

        var filteredAlarms = await FilterAlarmsByProductAsync(alarms, productCode, cancellationToken);

        return new DashboardRealtimeResponse
        {
            Summary = summary,
            LatestAlarms = filteredAlarms
                .OrderByDescending(x => x.OccurredAt)
                .Select(MapAlarm)
                .ToList()
        };
    }

    private async Task<List<TestRecord>> FilterByProductAsync(
        IReadOnlyList<TestRecord> records,
        string? productCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            return records.ToList();
        }

        var result = new List<TestRecord>();
        foreach (var record in records)
        {
            var serial = await _serialUnitRepository.GetBySnAsync(record.Sn, cancellationToken);
            if (serial is null)
            {
                continue;
            }

            var workOrder = await _workOrderRepository.GetByNoAsync(serial.WorkOrderNo, cancellationToken);
            if (workOrder is null)
            {
                continue;
            }

            if (string.Equals(workOrder.ProductCode, productCode, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(record);
            }
        }

        return result;
    }

    private async Task<List<AlarmEvent>> FilterAlarmsByProductAsync(
        IReadOnlyList<AlarmEvent> alarms,
        string? productCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            return alarms.ToList();
        }

        var result = new List<AlarmEvent>();
        foreach (var alarm in alarms)
        {
            var serial = await _serialUnitRepository.GetBySnAsync(alarm.Sn, cancellationToken);
            if (serial is null)
            {
                continue;
            }

            var workOrder = await _workOrderRepository.GetByNoAsync(serial.WorkOrderNo, cancellationToken);
            if (workOrder is null)
            {
                continue;
            }

            if (string.Equals(workOrder.ProductCode, productCode, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(alarm);
            }
        }

        return result;
    }

    private static SpcRuleResponse MapRule(SpcRule rule) =>
        new()
        {
            RuleCode = rule.RuleCode,
            MetricName = rule.MetricName,
            ProductCode = rule.ProductCode,
            StationCode = rule.StationCode,
            LowerLimit = rule.LowerLimit,
            UpperLimit = rule.UpperLimit,
            IsActive = rule.IsActive,
            CreatedAt = rule.CreatedAt
        };

    private static AlarmEventResponse MapAlarm(AlarmEvent alarmEvent) =>
        new()
        {
            AlarmCode = alarmEvent.AlarmCode,
            Sn = alarmEvent.Sn,
            StationCode = alarmEvent.StationCode,
            MetricName = alarmEvent.MetricName,
            MetricValue = alarmEvent.MetricValue,
            LowerLimit = alarmEvent.LowerLimit,
            UpperLimit = alarmEvent.UpperLimit,
            Severity = alarmEvent.Severity,
            Status = alarmEvent.Status,
            Message = alarmEvent.Message,
            OccurredAt = alarmEvent.OccurredAt
        };

    private static CommandResult Success(string message) =>
        new()
        {
            Success = true,
            Code = "MES-0000",
            Message = message
        };

    private static CommandResult Failed(string code, string message) =>
        new()
        {
            Success = false,
            Code = code,
            Message = message
        };
}
