using MES.Application.Contracts;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;

namespace MES.Domain.Tests;

public sealed class SpcServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ComputesYieldAndMetricStatistics()
    {
        var workOrderRepository = new InMemoryWorkOrderRepository();
        var serialUnitRepository = new InMemorySerialUnitRepository();
        var testRecordRepository = new InMemoryTestRecordRepository();
        var spcRuleRepository = new InMemorySpcRuleRepository();
        var alarmEventRepository = new InMemoryAlarmEventRepository();

        var service = new SpcService(
            testRecordRepository,
            serialUnitRepository,
            workOrderRepository,
            spcRuleRepository,
            alarmEventRepository);

        await workOrderRepository.AddAsync(new WorkOrder
        {
            WorkOrderNo = "WO-100",
            ProductCode = "PCB-A",
            PlannedQty = 100,
            Status = WorkOrderStatus.InProgress
        });

        await serialUnitRepository.AddAsync(new SerialUnit
        {
            Sn = "SN-100",
            WorkOrderNo = "WO-100",
            CurrentStationCode = "ST-ICT-01"
        });

        await serialUnitRepository.AddAsync(new SerialUnit
        {
            Sn = "SN-101",
            WorkOrderNo = "WO-100",
            CurrentStationCode = "ST-ICT-01"
        });

        await testRecordRepository.AddAsync(new TestRecord
        {
            Sn = "SN-100",
            StationCode = "ST-ICT-01",
            TestBatchId = "B-1",
            Passed = true,
            Metrics = new Dictionary<string, double>
            {
                ["Voltage"] = 3.25,
                ["Current"] = 0.35
            },
            TestedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        });

        await testRecordRepository.AddAsync(new TestRecord
        {
            Sn = "SN-101",
            StationCode = "ST-ICT-01",
            TestBatchId = "B-2",
            Passed = false,
            Metrics = new Dictionary<string, double>
            {
                ["Voltage"] = 3.45,
                ["Current"] = 0.45
            },
            TestedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        });

        var summary = await service.GetSummaryAsync(
            "PCB-A",
            "ST-ICT-01",
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow);

        Assert.Equal(2, summary.SampleCount);
        Assert.Equal(1, summary.PassCount);
        Assert.Equal(1, summary.FailCount);
        Assert.Equal(0.5d, summary.YieldRate, precision: 6);
        Assert.Equal(0.5d, summary.FirstPassYieldRate, precision: 6);

        var voltage = summary.Metrics.Single(x => x.MetricName == "Voltage");
        Assert.Equal(2, voltage.Count);
        Assert.Equal(3.35d, voltage.Mean, precision: 6);
        Assert.Equal(3.25d, voltage.Min, precision: 6);
        Assert.Equal(3.45d, voltage.Max, precision: 6);
    }

    [Fact]
    public async Task UploadTestResult_WhenMetricOutOfRange_RaisesAlarmEvent()
    {
        var workOrderRepository = new InMemoryWorkOrderRepository();
        var serialUnitRepository = new InMemorySerialUnitRepository();
        var testRecordRepository = new InMemoryTestRecordRepository();
        var traceEventRepository = new InMemoryTraceEventRepository();
        var stationRepository = new InMemoryStationRepository();
        var testFlowRepository = new InMemoryTestFlowRepository();
        var spcRuleRepository = new InMemorySpcRuleRepository();
        var alarmEventRepository = new InMemoryAlarmEventRepository();

        var mesService = new MesExecutionService(
            workOrderRepository,
            serialUnitRepository,
            testRecordRepository,
            traceEventRepository,
            stationRepository,
            testFlowRepository,
            spcRuleRepository,
            alarmEventRepository);

        await SeedFlowAsync(stationRepository, testFlowRepository);

        await workOrderRepository.AddAsync(new WorkOrder
        {
            WorkOrderNo = "WO-200",
            ProductCode = "PCB-A",
            PlannedQty = 20,
            Status = WorkOrderStatus.InProgress,
            TestFlowCode = "TF-PCB-A-V1"
        });

        await spcRuleRepository.AddAsync(new SpcRule
        {
            RuleCode = "RULE-VOLT-01",
            MetricName = "Voltage",
            ProductCode = "PCB-A",
            StationCode = "ST-ICT-01",
            LowerLimit = 3.00,
            UpperLimit = 3.30,
            IsActive = true
        });

        var passResult = await mesService.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-200",
            WorkOrderNo = "WO-200",
            StationCode = "ST-ASSY-01",
            OperatorId = "OP-1"
        });
        Assert.True(passResult.Success);

        var enterTestStationResult = await mesService.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-200",
            WorkOrderNo = "WO-200",
            StationCode = "ST-ICT-01",
            OperatorId = "OP-1"
        });
        Assert.True(enterTestStationResult.Success);

        var uploadResult = await mesService.UploadTestResultAsync(new UploadTestResultRequest
        {
            Sn = "SN-200",
            StationCode = "ST-ICT-01",
            TestBatchId = "BATCH-200",
            Passed = false,
            Metrics = new Dictionary<string, double>
            {
                ["Voltage"] = 3.55
            }
        });

        Assert.True(uploadResult.Success);

        var alarms = await alarmEventRepository.GetLatestAsync(10, "ST-ICT-01");
        Assert.Single(alarms);
        Assert.Equal("Voltage", alarms[0].MetricName);
        Assert.Equal(3.55d, alarms[0].MetricValue, precision: 6);

        var timeline = await traceEventRepository.GetBySnAsync("SN-200");
        Assert.Contains(timeline, x => x.EventType == "SpcAlarmRaised");
    }

    private static async Task SeedFlowAsync(
        InMemoryStationRepository stationRepository,
        InMemoryTestFlowRepository testFlowRepository)
    {
        await stationRepository.AddAsync(new Station
        {
            StationCode = "ST-ASSY-01",
            Name = "Assy",
            LineCode = "L1",
            IsTestStation = false
        });

        await stationRepository.AddAsync(new Station
        {
            StationCode = "ST-ICT-01",
            Name = "ICT",
            LineCode = "L1",
            IsTestStation = true
        });

        await testFlowRepository.AddAsync(new TestFlow
        {
            FlowCode = "TF-PCB-A-V1",
            Name = "Flow",
            ProductCode = "PCB-A",
            Version = "1",
            IsActive = true,
            Steps =
            [
                new RouteStep
                {
                    FlowCode = "TF-PCB-A-V1",
                    Sequence = 10,
                    StepCode = "ASSY",
                    StationCode = "ST-ASSY-01",
                    StepType = StepType.PassOnly
                },
                new RouteStep
                {
                    FlowCode = "TF-PCB-A-V1",
                    Sequence = 20,
                    StepCode = "ICT",
                    StationCode = "ST-ICT-01",
                    StepType = StepType.TestRequired
                }
            ]
        });
    }
}
