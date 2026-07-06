using MES.Application.Contracts;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Infrastructure.Seed;

public static class MesSeedData
{
    public static async Task InitializeAsync(
        IStationRepository stationRepository,
        ITestFlowRepository testFlowRepository)
    {
        if ((await stationRepository.GetAllAsync()).Count > 0)
        {
            return;
        }

        var stations = new[]
        {
            new Station { StationCode = "ST-ASSY-01", Name = "Assembly", LineCode = "LINE-01", IsTestStation = false },
            new Station { StationCode = "ST-ICT-01", Name = "ICT Test", LineCode = "LINE-01", IsTestStation = true },
            new Station { StationCode = "ST-FCT-01", Name = "FCT Test", LineCode = "LINE-01", IsTestStation = true }
        };

        foreach (var station in stations)
        {
            await stationRepository.AddAsync(station);
        }

        var flow = new TestFlow
        {
            FlowCode = "TF-PCB-A-V1",
            Name = "PCB-A Standard Test Flow",
            ProductCode = "PCB-A",
            Version = "1.0",
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
        };

        await testFlowRepository.AddAsync(flow);
    }
}
