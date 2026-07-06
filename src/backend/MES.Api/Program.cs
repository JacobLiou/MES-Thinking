using MES.Application.Contracts;
using MES.Application.Services;
using MES.Infrastructure.Repositories;
using MES.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var frontendOrigins = builder.Configuration.GetSection("Cors:FrontendOrigins").Get<string[]>()
    ?? ["https://localhost:7023", "http://localhost:5153", "https://localhost:44337", "http://localhost:20561"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
        policy.WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddSingleton<IWorkOrderRepository, InMemoryWorkOrderRepository>();
builder.Services.AddSingleton<ISerialUnitRepository, InMemorySerialUnitRepository>();
builder.Services.AddSingleton<ITestRecordRepository, InMemoryTestRecordRepository>();
builder.Services.AddSingleton<ITraceEventRepository, InMemoryTraceEventRepository>();
builder.Services.AddSingleton<IStationRepository, InMemoryStationRepository>();
builder.Services.AddSingleton<ITestFlowRepository, InMemoryTestFlowRepository>();
builder.Services.AddSingleton<IMesExecutionService, MesExecutionService>();
builder.Services.AddSingleton<IStationService, StationService>();
builder.Services.AddSingleton<ITestFlowService, TestFlowService>();

var app = builder.Build();

await MesSeedData.InitializeAsync(
    app.Services.GetRequiredService<IStationRepository>(),
    app.Services.GetRequiredService<ITestFlowRepository>());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevFrontend");
}

app.MapPost("/api/work-orders", async (CreateWorkOrderRequest request, IMesExecutionService service) =>
{
    var result = await service.CreateWorkOrderAsync(request);
    return Results.Ok(result);
})
.WithName("CreateWorkOrder")
.WithOpenApi();

app.MapPost("/api/stations", async (CreateStationRequest request, IStationService service) =>
{
    var result = await service.CreateStationAsync(request);
    return Results.Ok(result);
})
.WithName("CreateStation")
.WithOpenApi();

app.MapGet("/api/stations", async (IStationService service) =>
{
    var result = await service.GetStationsAsync();
    return Results.Ok(result);
})
.WithName("GetStations")
.WithOpenApi();

app.MapPost("/api/test-flows", async (CreateTestFlowRequest request, ITestFlowService service) =>
{
    var result = await service.CreateTestFlowAsync(request);
    return Results.Ok(result);
})
.WithName("CreateTestFlow")
.WithOpenApi();

app.MapGet("/api/test-flows", async (string? productCode, ITestFlowService service) =>
{
    var result = await service.GetTestFlowsAsync(productCode);
    return Results.Ok(result);
})
.WithName("GetTestFlows")
.WithOpenApi();

app.MapGet("/api/test-flows/{flowCode}", async (string flowCode, ITestFlowService service) =>
{
    var result = await service.GetTestFlowByCodeAsync(flowCode);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithName("GetTestFlowByCode")
.WithOpenApi();

app.MapPost("/api/test-flows/{flowCode}/activate", async (string flowCode, ITestFlowService service) =>
{
    var result = await service.ActivateTestFlowAsync(flowCode);
    return Results.Ok(result);
})
.WithName("ActivateTestFlow")
.WithOpenApi();

app.MapPost("/api/station/pass", async (StationPassRequest request, IMesExecutionService service) =>
{
    var result = await service.PassStationAsync(request);
    return Results.Ok(result);
})
.WithName("PassStation")
.WithOpenApi();

app.MapPost("/api/test-results", async (UploadTestResultRequest request, IMesExecutionService service) =>
{
    var result = await service.UploadTestResultAsync(request);
    return Results.Ok(result);
})
.WithName("UploadTestResult")
.WithOpenApi();

app.MapGet("/api/traceability/{sn}", async (string sn, IMesExecutionService service) =>
{
    var result = await service.GetTraceabilityAsync(sn);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithName("GetTraceability")
.WithOpenApi();

app.Run();
