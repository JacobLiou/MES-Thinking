using MES.Application.Contracts;
using MES.Domain.Entities;

namespace MES.Application.Services;

public sealed class StationService : IStationService
{
    private readonly IStationRepository _stationRepository;

    public StationService(IStationRepository stationRepository)
    {
        _stationRepository = stationRepository;
    }

    public async Task<CommandResult> CreateStationAsync(
        CreateStationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.StationCode)
            || string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.LineCode))
        {
            return Failed("MES-4001", "Station request is invalid.");
        }

        var existing = await _stationRepository.GetByCodeAsync(request.StationCode, cancellationToken);
        if (existing is not null)
        {
            return Failed("MES-4091", "Station already exists.");
        }

        var station = new Station
        {
            StationCode = request.StationCode.Trim(),
            Name = request.Name.Trim(),
            LineCode = request.LineCode.Trim(),
            IsTestStation = request.IsTestStation
        };

        await _stationRepository.AddAsync(station, cancellationToken);
        return Success("Station created.");
    }

    public async Task<IReadOnlyList<StationResponse>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        var stations = await _stationRepository.GetAllAsync(cancellationToken);
        return stations
            .Select(s => new StationResponse
            {
                StationCode = s.StationCode,
                Name = s.Name,
                LineCode = s.LineCode,
                IsTestStation = s.IsTestStation
            })
            .ToList();
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
