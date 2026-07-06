using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Domain.Services;

public static class TestFlowValidator
{
    public static FlowValidationResult ValidatePassStation(
        TestFlow flow,
        IReadOnlyDictionary<string, Station> stationsByCode,
        SerialUnit? serialUnit,
        string stationCode)
    {
        if (serialUnit?.Status == SerialStatus.TestedFail)
        {
            return FlowValidationResult.Fail("MES-4002", "SN is in failed state and cannot pass station.");
        }

        if (serialUnit?.PendingStepSequence is not null)
        {
            var pendingStep = flow.GetStepBySequence(serialUnit.PendingStepSequence.Value);
            if (pendingStep is not null && pendingStep.StepType == StepType.TestRequired)
            {
                return FlowValidationResult.Fail(
                    "MES-4002",
                    "SN is awaiting test result at current station before passing another station.");
            }
        }

        var expectedNext = flow.GetNextStep(serialUnit?.CompletedStepSequence);
        if (expectedNext is null)
        {
            return FlowValidationResult.Fail("MES-4002", "All route steps are already completed.");
        }

        if (!string.Equals(expectedNext.StationCode, stationCode, StringComparison.OrdinalIgnoreCase))
        {
            return FlowValidationResult.Fail(
                "MES-4002",
                $"Station '{stationCode}' does not match expected next step station '{expectedNext.StationCode}'.");
        }

        if (!stationsByCode.TryGetValue(stationCode, out var station))
        {
            return FlowValidationResult.Fail("MES-4003", $"Station '{stationCode}' is not registered.");
        }

        var stationTypeValid = expectedNext.StepType switch
        {
            StepType.TestRequired => station.IsTestStation,
            StepType.PassOnly => !station.IsTestStation,
            _ => false
        };

        if (!stationTypeValid)
        {
            return FlowValidationResult.Fail(
                "MES-4003",
                $"Station '{stationCode}' type does not match step type '{expectedNext.StepType}'.");
        }

        return FlowValidationResult.Ok();
    }

    public static FlowValidationResult ValidateTestUpload(
        TestFlow flow,
        SerialUnit serialUnit,
        string stationCode)
    {
        if (serialUnit.PendingStepSequence is null)
        {
            return FlowValidationResult.Fail(
                "MES-4002",
                "SN has not entered a test station. Pass station before uploading test result.");
        }

        var pendingStep = flow.GetStepBySequence(serialUnit.PendingStepSequence.Value);
        if (pendingStep is null)
        {
            return FlowValidationResult.Fail("MES-4002", "Pending step is not defined in the active test flow.");
        }

        if (pendingStep.StepType != StepType.TestRequired)
        {
            return FlowValidationResult.Fail("MES-4002", "Current step does not require a test upload.");
        }

        if (!string.Equals(pendingStep.StationCode, stationCode, StringComparison.OrdinalIgnoreCase))
        {
            return FlowValidationResult.Fail(
                "MES-4002",
                $"Test upload station '{stationCode}' does not match pending step station '{pendingStep.StationCode}'.");
        }

        return FlowValidationResult.Ok();
    }

    public static void ApplyPassStation(SerialUnit serialUnit, RouteStep step)
    {
        serialUnit.CurrentStationCode = step.StationCode;
        serialUnit.Status = SerialStatus.InProcess;
        serialUnit.UpdatedAt = DateTimeOffset.UtcNow;

        if (step.StepType == StepType.PassOnly)
        {
            serialUnit.CompletedStepSequence = step.Sequence;
            serialUnit.PendingStepSequence = null;
        }
        else
        {
            serialUnit.PendingStepSequence = step.Sequence;
        }
    }

    public static void ApplyTestResult(SerialUnit serialUnit, TestFlow flow, RouteStep step, bool passed)
    {
        serialUnit.LastTestPassed = passed;
        serialUnit.UpdatedAt = DateTimeOffset.UtcNow;

        if (!passed)
        {
            serialUnit.Status = SerialStatus.TestedFail;
            return;
        }

        serialUnit.CompletedStepSequence = step.Sequence;
        serialUnit.PendingStepSequence = null;
        serialUnit.Status = SerialStatus.TestedPass;

        var lastStep = flow.GetLastStep();
        if (lastStep is not null && lastStep.Sequence == step.Sequence)
        {
            serialUnit.Status = SerialStatus.Done;
        }
        else
        {
            serialUnit.Status = SerialStatus.InProcess;
        }
    }
}
