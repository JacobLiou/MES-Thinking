using MES.Application.Contracts;
using MES.Application.Services;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Repositories;

namespace MES.Domain.Tests;

public sealed class TestFlowServiceTests
{
    [Fact]
    public async Task ActivateTestFlow_DeactivatesOtherFlowsForSameProduct()
    {
        var stationRepository = new InMemoryStationRepository();
        var testFlowRepository = new InMemoryTestFlowRepository();
        var service = new TestFlowService(testFlowRepository, stationRepository);

        await stationRepository.AddAsync(new Station
        {
            StationCode = "ST-ASSY-01",
            Name = "Assy",
            LineCode = "L1",
            IsTestStation = false
        });

        await testFlowRepository.AddAsync(CreateFlow("TF-V1", "PCB-A", isActive: true));
        await testFlowRepository.AddAsync(CreateFlow("TF-V2", "PCB-A", isActive: false));

        var result = await service.ActivateTestFlowAsync("TF-V2");

        Assert.True(result.Success);
        var flows = await testFlowRepository.GetAllAsync("PCB-A");
        Assert.False(flows.Single(f => f.FlowCode == "TF-V1").IsActive);
        Assert.True(flows.Single(f => f.FlowCode == "TF-V2").IsActive);
    }

    [Fact]
    public async Task CreateTestFlow_RejectsNonMonotonicSequences()
    {
        var stationRepository = new InMemoryStationRepository();
        var testFlowRepository = new InMemoryTestFlowRepository();
        var service = new TestFlowService(testFlowRepository, stationRepository);

        await stationRepository.AddAsync(new Station
        {
            StationCode = "ST-ASSY-01",
            Name = "Assy",
            LineCode = "L1",
            IsTestStation = false
        });

        var result = await service.CreateTestFlowAsync(new CreateTestFlowRequest
        {
            FlowCode = "TF-BAD",
            Name = "Bad",
            ProductCode = "PCB-B",
            Version = "1",
            Steps =
            [
                new CreateRouteStepRequest { Sequence = 10, StepCode = "A", StationCode = "ST-ASSY-01" },
                new CreateRouteStepRequest { Sequence = 10, StepCode = "B", StationCode = "ST-ASSY-01" }
            ]
        });

        Assert.False(result.Success);
        Assert.Equal("MES-4001", result.Code);
    }

    private static TestFlow CreateFlow(string flowCode, string productCode, bool isActive) =>
        new()
        {
            FlowCode = flowCode,
            Name = flowCode,
            ProductCode = productCode,
            Version = "1",
            IsActive = isActive,
            Steps =
            [
                new RouteStep
                {
                    FlowCode = flowCode,
                    Sequence = 10,
                    StepCode = "ASSY",
                    StationCode = "ST-ASSY-01",
                    StepType = StepType.PassOnly
                }
            ]
        };
}
