using System.Net.Http.Json;
using MES.Web.Models;

namespace MES.Web.Services;

public sealed class MesApiClient
{
    private readonly HttpClient _httpClient;

    public MesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/stations", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<StationResponse>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<StationResponse>>("/api/stations", cancellationToken);
        return result ?? [];
    }

    public async Task<IReadOnlyList<TestFlowResponse>> GetTestFlowsAsync(
        string? productCode = null,
        CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(productCode)
            ? "/api/test-flows"
            : $"/api/test-flows?productCode={Uri.EscapeDataString(productCode)}";

        var result = await _httpClient.GetFromJsonAsync<List<TestFlowResponse>>(url, cancellationToken);
        return result ?? [];
    }

    public async Task<CommandResult> CreateWorkOrderAsync(
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/work-orders", request, cancellationToken);
        return await ReadCommandResultAsync(response, cancellationToken);
    }

    public async Task<CommandResult> PassStationAsync(
        StationPassRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/station/pass", request, cancellationToken);
        return await ReadCommandResultAsync(response, cancellationToken);
    }

    public async Task<CommandResult> UploadTestResultAsync(
        UploadTestResultRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/test-results", request, cancellationToken);
        return await ReadCommandResultAsync(response, cancellationToken);
    }

    public async Task<TraceabilityResponse?> GetTraceabilityAsync(
        string sn,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<TraceabilityResponse>(
            $"/api/traceability/{Uri.EscapeDataString(sn)}",
            cancellationToken);
    }

    private static async Task<CommandResult> ReadCommandResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var result = await response.Content.ReadFromJsonAsync<CommandResult>(cancellationToken);
        return result ?? new CommandResult
        {
            Success = false,
            Code = "MES-5001",
            Message = $"Unexpected API response ({(int)response.StatusCode})."
        };
    }
}
