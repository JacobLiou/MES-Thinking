using MES.Application.Contracts;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;

namespace MES.Domain.Tests;

public sealed class MesExecutionServiceFlowTests
{
    [Fact]
    public async Task PassStation_RejectsSkipAfterFirstStepCompleted()
    {
        var workOrderRepository = new InMemoryWorkOrderRepository();
        var serialUnitRepository = new InMemorySerialUnitRepository();
        var testRecordRepository = new InMemoryTestRecordRepository();
        var traceEventRepository = new InMemoryTraceEventRepository();
        var stationRepository = new InMemoryStationRepository();
        var testFlowRepository = new InMemoryTestFlowRepository();
        var spcRuleRepository = new InMemorySpcRuleRepository();
        var alarmEventRepository = new InMemoryAlarmEventRepository();
        var service = new MesExecutionService(
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
            WorkOrderNo = "WO-1",
            ProductCode = "PCB-A",
            PlannedQty = 10,
            Status = WorkOrderStatus.Released,
            TestFlowCode = "TF-PCB-A-V1"
        });

        var firstPass = await service.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-001",
            WorkOrderNo = "WO-1",
            StationCode = "ST-ASSY-01",
            OperatorId = "OP1"
        });
        Assert.True(firstPass.Success);

        var skipPass = await service.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-001",
            WorkOrderNo = "WO-1",
            StationCode = "ST-FCT-01",
            OperatorId = "OP1"
        });

        Assert.False(skipPass.Success);
        Assert.Equal("MES-4002", skipPass.Code);
    }

    [Fact]
    public async Task UploadTestResult_RequiresStationPassFirst()
    {
        var workOrderRepository = new InMemoryWorkOrderRepository();
        var serialUnitRepository = new InMemorySerialUnitRepository();
        var testRecordRepository = new InMemoryTestRecordRepository();
        var traceEventRepository = new InMemoryTraceEventRepository();
        var stationRepository = new InMemoryStationRepository();
        var testFlowRepository = new InMemoryTestFlowRepository();
        var spcRuleRepository = new InMemorySpcRuleRepository();
        var alarmEventRepository = new InMemoryAlarmEventRepository();
        var service = new MesExecutionService(
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
            WorkOrderNo = "WO-1",
            ProductCode = "PCB-A",
            PlannedQty = 10,
            Status = WorkOrderStatus.Released,
            TestFlowCode = "TF-PCB-A-V1"
        });

        await service.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-002",
            WorkOrderNo = "WO-1",
            StationCode = "ST-ASSY-01",
            OperatorId = "OP1"
        });

        await serialUnitRepository.AddAsync(new SerialUnit
        {
            Sn = "SN-003",
            WorkOrderNo = "WO-1",
            CurrentStationCode = "ST-ICT-01",
            CompletedStepSequence = 10
        });

        var result = await service.UploadTestResultAsync(new UploadTestResultRequest
        {
            Sn = "SN-003",
            StationCode = "ST-ICT-01",
            TestBatchId = "B1",
            Passed = true
        });

        Assert.False(result.Success);
        Assert.Equal("MES-4002", result.Code);
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
        await stationRepository.AddAsync(new Station
        {
            StationCode = "ST-FCT-01",
            Name = "FCT",
            LineCode = "L1",
            IsTestStation = true
        });

        await testFlowRepository.AddAsync(new TestFlow
        {
            FlowCode = "TF-PCB-A-V1",
            Name = "PCB-A",
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
                },
                new RouteStep
                {
                    FlowCode = "TF-PCB-A-V1",
                    Sequence = 30,
                    StepCode = "FCT",
                    StationCode = "ST-FCT-01",
                    StepType = StepType.TestRequired
                }
            ]
        });
    }
}
