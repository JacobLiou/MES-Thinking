using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Domain.Services;

namespace MES.Domain.Tests;

public sealed class TestFlowValidatorTests
{
    private static readonly TestFlow Flow = new()
    {
        FlowCode = "TF-1",
        Name = "Test",
        ProductCode = "PCB-A",
        Version = "1",
        IsActive = true,
        Steps =
        [
            new RouteStep
            {
                FlowCode = "TF-1",
                Sequence = 10,
                StepCode = "ASSY",
                StationCode = "ST-ASSY-01",
                StepType = StepType.PassOnly
            },
            new RouteStep
            {
                FlowCode = "TF-1",
                Sequence = 20,
                StepCode = "ICT",
                StationCode = "ST-ICT-01",
                StepType = StepType.TestRequired
            },
            new RouteStep
            {
                FlowCode = "TF-1",
                Sequence = 30,
                StepCode = "FCT",
                StationCode = "ST-FCT-01",
                StepType = StepType.TestRequired
            }
        ]
    };

    private static readonly Dictionary<string, Station> Stations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ST-ASSY-01"] = new() { StationCode = "ST-ASSY-01", Name = "Assy", LineCode = "L1", IsTestStation = false },
        ["ST-ICT-01"] = new() { StationCode = "ST-ICT-01", Name = "ICT", LineCode = "L1", IsTestStation = true },
        ["ST-FCT-01"] = new() { StationCode = "ST-FCT-01", Name = "FCT", LineCode = "L1", IsTestStation = true }
    };

    [Fact]
    public void ValidatePassStation_FirstStep_AcceptsExpectedStation()
    {
        var result = TestFlowValidator.ValidatePassStation(Flow, Stations, null, "ST-ASSY-01");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidatePassStation_SkipStation_ReturnsMes4002()
    {
        var result = TestFlowValidator.ValidatePassStation(Flow, Stations, null, "ST-ICT-01");

        Assert.False(result.IsValid);
        Assert.Equal("MES-4002", result.ErrorCode);
    }

    [Fact]
    public void ValidatePassStation_FailedSn_ReturnsMes4002()
    {
        var serial = new SerialUnit
        {
            Sn = "SN1",
            WorkOrderNo = "WO1",
            CurrentStationCode = "ST-ICT-01",
            Status = SerialStatus.TestedFail,
            CompletedStepSequence = 10
        };

        var result = TestFlowValidator.ValidatePassStation(Flow, Stations, serial, "ST-FCT-01");

        Assert.False(result.IsValid);
        Assert.Equal("MES-4002", result.ErrorCode);
    }

    [Fact]
    public void ApplyPassStation_TestStep_SetsPendingWithoutCompleting()
    {
        var serial = new SerialUnit
        {
            Sn = "SN1",
            WorkOrderNo = "WO1",
            CurrentStationCode = "ST-ASSY-01",
            CompletedStepSequence = 10
        };

        var ictStep = Flow.GetStepBySequence(20)!;
        TestFlowValidator.ApplyPassStation(serial, ictStep);

        Assert.Equal(20, serial.PendingStepSequence);
        Assert.Equal(10, serial.CompletedStepSequence);
    }

    [Fact]
    public void ValidateTestUpload_WithoutPass_ReturnsMes4002()
    {
        var serial = new SerialUnit
        {
            Sn = "SN1",
            WorkOrderNo = "WO1",
            CurrentStationCode = "ST-ICT-01",
            CompletedStepSequence = 10
        };

        var result = TestFlowValidator.ValidateTestUpload(Flow, serial, "ST-ICT-01");

        Assert.False(result.IsValid);
        Assert.Equal("MES-4002", result.ErrorCode);
    }

    [Fact]
    public void ApplyTestResult_Pass_AdvancesCompletedSequence()
    {
        var serial = new SerialUnit
        {
            Sn = "SN1",
            WorkOrderNo = "WO1",
            CurrentStationCode = "ST-ICT-01",
            PendingStepSequence = 20,
            CompletedStepSequence = 10
        };

        var ictStep = Flow.GetStepBySequence(20)!;
        TestFlowValidator.ApplyTestResult(serial, Flow, ictStep, passed: true);

        Assert.Equal(20, serial.CompletedStepSequence);
        Assert.Null(serial.PendingStepSequence);
        Assert.Equal(SerialStatus.InProcess, serial.Status);
    }

    [Fact]
    public void ApplyTestResult_Fail_LocksSn()
    {
        var serial = new SerialUnit
        {
            Sn = "SN1",
            WorkOrderNo = "WO1",
            CurrentStationCode = "ST-ICT-01",
            PendingStepSequence = 20,
            CompletedStepSequence = 10
        };

        var ictStep = Flow.GetStepBySequence(20)!;
        TestFlowValidator.ApplyTestResult(serial, Flow, ictStep, passed: false);

        Assert.Equal(SerialStatus.TestedFail, serial.Status);
        Assert.Equal(10, serial.CompletedStepSequence);
    }

    [Fact]
    public void ApplyTestResult_LastStepPass_SetsDone()
    {
        var serial = new SerialUnit
        {
            Sn = "SN1",
            WorkOrderNo = "WO1",
            CurrentStationCode = "ST-FCT-01",
            PendingStepSequence = 30,
            CompletedStepSequence = 20
        };

        var fctStep = Flow.GetStepBySequence(30)!;
        TestFlowValidator.ApplyTestResult(serial, Flow, fctStep, passed: true);

        Assert.Equal(SerialStatus.Done, serial.Status);
        Assert.Equal(30, serial.CompletedStepSequence);
    }
}
