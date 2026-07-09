using MES.Application.Contracts;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Services;

namespace MES.Application.Services;

public sealed class MesExecutionService : IMesExecutionService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ISerialUnitRepository _serialUnitRepository;
    private readonly ITestRecordRepository _testRecordRepository;
    private readonly ITraceEventRepository _traceEventRepository;
    private readonly IStationRepository _stationRepository;
    private readonly ITestFlowRepository _testFlowRepository;
    private readonly ISpcRuleRepository _spcRuleRepository;
    private readonly IAlarmEventRepository _alarmEventRepository;

    public MesExecutionService(
        IWorkOrderRepository workOrderRepository,
        ISerialUnitRepository serialUnitRepository,
        ITestRecordRepository testRecordRepository,
        ITraceEventRepository traceEventRepository,
        IStationRepository stationRepository,
        ITestFlowRepository testFlowRepository,
        ISpcRuleRepository spcRuleRepository,
        IAlarmEventRepository alarmEventRepository)
    {
        _workOrderRepository = workOrderRepository;
        _serialUnitRepository = serialUnitRepository;
        _testRecordRepository = testRecordRepository;
        _traceEventRepository = traceEventRepository;
        _stationRepository = stationRepository;
        _testFlowRepository = testFlowRepository;
        _spcRuleRepository = spcRuleRepository;
        _alarmEventRepository = alarmEventRepository;
    }

    public async Task<CommandResult> CreateWorkOrderAsync(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNo) || string.IsNullOrWhiteSpace(request.ProductCode))
        {
            return Failed("MES-4001", "WorkOrderNo or ProductCode is invalid.");
        }

        var exists = await _workOrderRepository.GetByNoAsync(request.WorkOrderNo, cancellationToken);
        if (exists is not null)
        {
            return Failed("MES-4091", "Work order already exists.");
        }

        var activeFlow = await _testFlowRepository.GetActiveByProductCodeAsync(request.ProductCode, cancellationToken);

        var workOrder = new WorkOrder
        {
            WorkOrderNo = request.WorkOrderNo,
            ProductCode = request.ProductCode,
            PlannedQty = request.PlannedQty,
            Status = WorkOrderStatus.Released,
            TestFlowCode = activeFlow?.FlowCode
        };

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);
        return Success("Work order created.");
    }

    public async Task<CommandResult> PassStationAsync(StationPassRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Sn)
            || string.IsNullOrWhiteSpace(request.WorkOrderNo)
            || string.IsNullOrWhiteSpace(request.StationCode)
            || string.IsNullOrWhiteSpace(request.OperatorId))
        {
            return Failed("MES-4001", "Station pass request is invalid.");
        }

        var workOrder = await _workOrderRepository.GetByNoAsync(request.WorkOrderNo, cancellationToken);
        if (workOrder is null)
        {
            return Failed("MES-4002", "Work order does not exist.");
        }

        var flow = await ResolveTestFlowAsync(workOrder, cancellationToken);
        if (flow is null)
        {
            return Failed("MES-4004", "No active test flow is configured for this product.");
        }

        if (workOrder.Status == WorkOrderStatus.Released)
        {
            workOrder.Status = WorkOrderStatus.InProgress;
            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);
        }

        var serialUnit = await _serialUnitRepository.GetBySnAsync(request.Sn, cancellationToken);
        var stationsByCode = await BuildStationLookupAsync(cancellationToken);

        var validation = TestFlowValidator.ValidatePassStation(
            flow,
            stationsByCode,
            serialUnit,
            request.StationCode);

        if (!validation.IsValid)
        {
            return Failed(validation.ErrorCode!, validation.ErrorMessage!);
        }

        var expectedNext = flow.GetNextStep(serialUnit?.CompletedStepSequence)!;

        if (serialUnit is null)
        {
            serialUnit = new SerialUnit
            {
                Sn = request.Sn,
                WorkOrderNo = request.WorkOrderNo,
                CurrentStationCode = request.StationCode
            };
            TestFlowValidator.ApplyPassStation(serialUnit, expectedNext);
            await _serialUnitRepository.AddAsync(serialUnit, cancellationToken);
        }
        else
        {
            TestFlowValidator.ApplyPassStation(serialUnit, expectedNext);
            await _serialUnitRepository.UpdateAsync(serialUnit, cancellationToken);
        }

        await _traceEventRepository.AddAsync(new TraceEvent
        {
            Sn = request.Sn,
            EventType = "StationPassed",
            StationCode = request.StationCode,
            OperatorId = request.OperatorId,
            Message = expectedNext.StepType == StepType.TestRequired
                ? "SN entered test station; awaiting test result."
                : "SN pass station accepted."
        }, cancellationToken);

        return Success("Station pass accepted.");
    }

    public async Task<CommandResult> UploadTestResultAsync(UploadTestResultRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Sn)
            || string.IsNullOrWhiteSpace(request.StationCode)
            || string.IsNullOrWhiteSpace(request.TestBatchId))
        {
            return Failed("MES-4001", "Test result request is invalid.");
        }

        var duplicate = await _testRecordRepository.ExistsAsync(
            request.Sn,
            request.StationCode,
            request.TestBatchId,
            cancellationToken);

        if (duplicate)
        {
            return Failed("MES-4091", "Duplicate test result upload.");
        }

        var serialUnit = await _serialUnitRepository.GetBySnAsync(request.Sn, cancellationToken);
        if (serialUnit is null)
        {
            return Failed("MES-4002", "SN has not entered process.");
        }

        var workOrder = await _workOrderRepository.GetByNoAsync(serialUnit.WorkOrderNo, cancellationToken);
        if (workOrder is null)
        {
            return Failed("MES-4002", "Work order does not exist.");
        }

        var flow = await ResolveTestFlowAsync(workOrder, cancellationToken);
        if (flow is null)
        {
            return Failed("MES-4004", "No active test flow is configured for this product.");
        }

        var validation = TestFlowValidator.ValidateTestUpload(flow, serialUnit, request.StationCode);
        if (!validation.IsValid)
        {
            return Failed(validation.ErrorCode!, validation.ErrorMessage!);
        }

        var pendingStep = flow.GetStepBySequence(serialUnit.PendingStepSequence!.Value)!;

        var testRecord = new TestRecord
        {
            Sn = request.Sn,
            StationCode = request.StationCode,
            TestBatchId = request.TestBatchId,
            Passed = request.Passed,
            Metrics = request.Metrics ?? new Dictionary<string, double>(),
            RawPayload = request.RawPayload,
            TestedAt = DateTimeOffset.UtcNow
        };

        await _testRecordRepository.AddAsync(testRecord, cancellationToken);
        TestFlowValidator.ApplyTestResult(serialUnit, flow, pendingStep, request.Passed);
        await _serialUnitRepository.UpdateAsync(serialUnit, cancellationToken);

        await EvaluateSpcRulesAsync(
            testRecord,
            workOrder.ProductCode,
            cancellationToken);

        await _traceEventRepository.AddAsync(new TraceEvent
        {
            Sn = request.Sn,
            EventType = "TestUploaded",
            StationCode = request.StationCode,
            OperatorId = "SYSTEM",
            Message = request.Passed ? "Test passed." : "Test failed."
        }, cancellationToken);

        return Success("Test result uploaded.");
    }

    private async Task EvaluateSpcRulesAsync(
        TestRecord testRecord,
        string productCode,
        CancellationToken cancellationToken)
    {
        if (testRecord.Metrics.Count == 0)
        {
            return;
        }

        var rules = await _spcRuleRepository.GetAllAsync(productCode, testRecord.StationCode, cancellationToken);
        var activeRules = rules.Where(x => x.IsActive).ToList();

        if (activeRules.Count == 0)
        {
            return;
        }

        foreach (var metric in testRecord.Metrics)
        {
            var matchedRules = activeRules.Where(rule =>
                string.Equals(rule.MetricName, metric.Key, StringComparison.OrdinalIgnoreCase));

            foreach (var rule in matchedRules)
            {
                var lowerViolated = rule.LowerLimit.HasValue && metric.Value < rule.LowerLimit.Value;
                var upperViolated = rule.UpperLimit.HasValue && metric.Value > rule.UpperLimit.Value;

                if (!lowerViolated && !upperViolated)
                {
                    continue;
                }

                var alarm = new AlarmEvent
                {
                    AlarmCode = $"ALM-{Guid.NewGuid():N}",
                    Sn = testRecord.Sn,
                    StationCode = testRecord.StationCode,
                    MetricName = metric.Key,
                    MetricValue = metric.Value,
                    LowerLimit = rule.LowerLimit,
                    UpperLimit = rule.UpperLimit,
                    Severity = "Warning",
                    Status = "New",
                    Message = $"SPC rule '{rule.RuleCode}' violated for metric '{metric.Key}'."
                };

                await _alarmEventRepository.AddAsync(alarm, cancellationToken);
                await _traceEventRepository.AddAsync(new TraceEvent
                {
                    Sn = testRecord.Sn,
                    EventType = "SpcAlarmRaised",
                    StationCode = testRecord.StationCode,
                    OperatorId = "SYSTEM",
                    Message = alarm.Message
                }, cancellationToken);
            }
        }
    }

    public async Task<TraceabilityResponse?> GetTraceabilityAsync(string sn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sn))
        {
            return null;
        }

        var serialUnit = await _serialUnitRepository.GetBySnAsync(sn, cancellationToken);
        if (serialUnit is null)
        {
            return null;
        }

        TestFlow? flow = null;
        string? nextExpectedStation = null;
        var workOrder = await _workOrderRepository.GetByNoAsync(serialUnit.WorkOrderNo, cancellationToken);
        if (workOrder is not null)
        {
            flow = await ResolveTestFlowAsync(workOrder, cancellationToken);
            if (flow is not null)
            {
                var nextStep = flow.GetNextStep(serialUnit.CompletedStepSequence);
                nextExpectedStation = nextStep?.StationCode;
            }
        }

        var timeline = await _traceEventRepository.GetBySnAsync(sn, cancellationToken);
        var testResults = await _testRecordRepository.GetBySnAsync(sn, cancellationToken);

        return new TraceabilityResponse
        {
            Sn = serialUnit.Sn,
            WorkOrderNo = serialUnit.WorkOrderNo,
            CurrentStationCode = serialUnit.CurrentStationCode,
            CurrentStatus = serialUnit.Status.ToString(),
            ActiveFlowCode = flow?.FlowCode,
            CompletedStepSequence = serialUnit.CompletedStepSequence,
            NextExpectedStation = nextExpectedStation,
            Timeline = timeline
                .OrderBy(x => x.OccurredAt)
                .Select(x => new TraceTimelineItem
                {
                    EventType = x.EventType,
                    StationCode = x.StationCode,
                    OperatorId = x.OperatorId,
                    Message = x.Message,
                    OccurredAt = x.OccurredAt
                })
                .ToList(),
            TestResults = testResults
                .OrderBy(x => x.TestedAt)
                .Select(x => new TestResultItem
                {
                    StationCode = x.StationCode,
                    TestBatchId = x.TestBatchId,
                    Passed = x.Passed,
                    Metrics = x.Metrics,
                    TestedAt = x.TestedAt
                })
                .ToList()
        };
    }

    private async Task<TestFlow?> ResolveTestFlowAsync(WorkOrder workOrder, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(workOrder.TestFlowCode))
        {
            var byCode = await _testFlowRepository.GetByCodeAsync(workOrder.TestFlowCode, cancellationToken);
            if (byCode is not null)
            {
                return byCode;
            }
        }

        return await _testFlowRepository.GetActiveByProductCodeAsync(workOrder.ProductCode, cancellationToken);
    }

    private async Task<Dictionary<string, Station>> BuildStationLookupAsync(CancellationToken cancellationToken)
    {
        var stations = await _stationRepository.GetAllAsync(cancellationToken);
        return stations.ToDictionary(s => s.StationCode, StringComparer.OrdinalIgnoreCase);
    }

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
