using MES.Application.Contracts;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Application.Services;

public sealed class TestFlowService : ITestFlowService
{
    private readonly ITestFlowRepository _testFlowRepository;
    private readonly IStationRepository _stationRepository;

    public TestFlowService(ITestFlowRepository testFlowRepository, IStationRepository stationRepository)
    {
        _testFlowRepository = testFlowRepository;
        _stationRepository = stationRepository;
    }

    public async Task<CommandResult> CreateTestFlowAsync(
        CreateTestFlowRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FlowCode)
            || string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.ProductCode)
            || string.IsNullOrWhiteSpace(request.Version)
            || request.Steps.Count == 0)
        {
            return Failed("MES-4001", "Test flow request is invalid.");
        }

        var existing = await _testFlowRepository.GetByCodeAsync(request.FlowCode, cancellationToken);
        if (existing is not null)
        {
            return Failed("MES-4091", "Test flow already exists.");
        }

        var orderedSteps = request.Steps.OrderBy(s => s.Sequence).ToList();
        for (var i = 1; i < orderedSteps.Count; i++)
        {
            if (orderedSteps[i].Sequence <= orderedSteps[i - 1].Sequence)
            {
                return Failed("MES-4001", "Route step sequences must be strictly increasing.");
            }
        }

        var routeSteps = new List<RouteStep>();
        foreach (var step in orderedSteps)
        {
            if (string.IsNullOrWhiteSpace(step.StepCode) || string.IsNullOrWhiteSpace(step.StationCode))
            {
                return Failed("MES-4001", "Route step code or station code is invalid.");
            }

            var station = await _stationRepository.GetByCodeAsync(step.StationCode, cancellationToken);
            if (station is null)
            {
                return Failed("MES-4003", $"Station '{step.StationCode}' is not registered.");
            }

            if (!Enum.TryParse<StepType>(step.StepType, ignoreCase: true, out var stepType))
            {
                return Failed("MES-4001", $"Step type '{step.StepType}' is invalid.");
            }

            var stationTypeValid = stepType switch
            {
                StepType.TestRequired => station.IsTestStation,
                StepType.PassOnly => !station.IsTestStation,
                _ => false
            };

            if (!stationTypeValid)
            {
                return Failed(
                    "MES-4003",
                    $"Station '{step.StationCode}' type does not match step type '{stepType}'.");
            }

            routeSteps.Add(new RouteStep
            {
                FlowCode = request.FlowCode.Trim(),
                Sequence = step.Sequence,
                StepCode = step.StepCode.Trim(),
                StationCode = step.StationCode.Trim(),
                StepType = stepType,
                AllowRework = step.AllowRework
            });
        }

        if (request.IsActive)
        {
            await _testFlowRepository.DeactivateByProductCodeAsync(
                request.ProductCode.Trim(),
                request.FlowCode.Trim(),
                cancellationToken);
        }

        var testFlow = new TestFlow
        {
            FlowCode = request.FlowCode.Trim(),
            Name = request.Name.Trim(),
            ProductCode = request.ProductCode.Trim(),
            Version = request.Version.Trim(),
            IsActive = request.IsActive,
            Steps = routeSteps
        };

        await _testFlowRepository.AddAsync(testFlow, cancellationToken);
        return Success("Test flow created.");
    }

    public async Task<IReadOnlyList<TestFlowResponse>> GetTestFlowsAsync(
        string? productCode,
        CancellationToken cancellationToken = default)
    {
        var flows = await _testFlowRepository.GetAllAsync(productCode, cancellationToken);
        return flows.Select(MapToResponse).ToList();
    }

    public async Task<TestFlowResponse?> GetTestFlowByCodeAsync(
        string flowCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flowCode))
        {
            return null;
        }

        var flow = await _testFlowRepository.GetByCodeAsync(flowCode, cancellationToken);
        return flow is null ? null : MapToResponse(flow);
    }

    public async Task<CommandResult> ActivateTestFlowAsync(
        string flowCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flowCode))
        {
            return Failed("MES-4001", "Flow code is invalid.");
        }

        var flow = await _testFlowRepository.GetByCodeAsync(flowCode, cancellationToken);
        if (flow is null)
        {
            return Failed("MES-4002", "Test flow does not exist.");
        }

        await _testFlowRepository.DeactivateByProductCodeAsync(flow.ProductCode, flow.FlowCode, cancellationToken);
        flow.IsActive = true;
        await _testFlowRepository.UpdateAsync(flow, cancellationToken);
        return Success("Test flow activated.");
    }

    private static TestFlowResponse MapToResponse(TestFlow flow) =>
        new()
        {
            FlowCode = flow.FlowCode,
            Name = flow.Name,
            ProductCode = flow.ProductCode,
            Version = flow.Version,
            IsActive = flow.IsActive,
            CreatedAt = flow.CreatedAt,
            Steps = flow.Steps
                .OrderBy(s => s.Sequence)
                .Select(s => new RouteStepResponse
                {
                    Sequence = s.Sequence,
                    StepCode = s.StepCode,
                    StationCode = s.StationCode,
                    StepType = s.StepType.ToString(),
                    AllowRework = s.AllowRework
                })
                .ToList()
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
