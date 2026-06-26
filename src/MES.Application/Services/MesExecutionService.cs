using MES.Application.Contracts;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Application.Services;

public sealed class MesExecutionService : IMesExecutionService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ISerialUnitRepository _serialUnitRepository;
    private readonly ITestRecordRepository _testRecordRepository;
    private readonly ITraceEventRepository _traceEventRepository;

    public MesExecutionService(
        IWorkOrderRepository workOrderRepository,
        ISerialUnitRepository serialUnitRepository,
        ITestRecordRepository testRecordRepository,
        ITraceEventRepository traceEventRepository)
    {
        _workOrderRepository = workOrderRepository;
        _serialUnitRepository = serialUnitRepository;
        _testRecordRepository = testRecordRepository;
        _traceEventRepository = traceEventRepository;
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

        var workOrder = new WorkOrder
        {
            WorkOrderNo = request.WorkOrderNo,
            ProductCode = request.ProductCode,
            PlannedQty = request.PlannedQty,
            Status = WorkOrderStatus.Released
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

        if (workOrder.Status == WorkOrderStatus.Released)
        {
            workOrder.Status = WorkOrderStatus.InProgress;
            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);
        }

        var serialUnit = await _serialUnitRepository.GetBySnAsync(request.Sn, cancellationToken);
        if (serialUnit is null)
        {
            serialUnit = new SerialUnit
            {
                Sn = request.Sn,
                WorkOrderNo = request.WorkOrderNo,
                CurrentStationCode = request.StationCode,
                Status = SerialStatus.InProcess,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            await _serialUnitRepository.AddAsync(serialUnit, cancellationToken);
        }
        else
        {
            serialUnit.CurrentStationCode = request.StationCode;
            serialUnit.Status = SerialStatus.InProcess;
            serialUnit.UpdatedAt = DateTimeOffset.UtcNow;
            await _serialUnitRepository.UpdateAsync(serialUnit, cancellationToken);
        }

        await _traceEventRepository.AddAsync(new TraceEvent
        {
            Sn = request.Sn,
            EventType = "StationPassed",
            StationCode = request.StationCode,
            OperatorId = request.OperatorId,
            Message = "SN pass station accepted."
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

        serialUnit.LastTestPassed = request.Passed;
        serialUnit.Status = request.Passed ? SerialStatus.TestedPass : SerialStatus.TestedFail;
        serialUnit.UpdatedAt = DateTimeOffset.UtcNow;
        await _serialUnitRepository.UpdateAsync(serialUnit, cancellationToken);

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

        var timeline = await _traceEventRepository.GetBySnAsync(sn, cancellationToken);
        var testResults = await _testRecordRepository.GetBySnAsync(sn, cancellationToken);

        return new TraceabilityResponse
        {
            Sn = serialUnit.Sn,
            WorkOrderNo = serialUnit.WorkOrderNo,
            CurrentStationCode = serialUnit.CurrentStationCode,
            CurrentStatus = serialUnit.Status.ToString(),
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
