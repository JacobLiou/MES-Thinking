using MES.Application.Contracts;
using MES.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MES.Api.Hubs;
using MES.Infrastructure.Persistence;
using MES.Infrastructure.Repositories;
using MES.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var frontendOrigins = builder.Configuration.GetSection("Cors:FrontendOrigins").Get<string[]>()
    ?? ["https://localhost:7023", "http://localhost:5153", "https://localhost:44337", "http://localhost:20561"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
        policy.WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var useInMemory = builder.Configuration.GetValue<bool>("Persistence:UseInMemory");

if (useInMemory)
{
    builder.Services.AddSingleton<IWorkOrderRepository, InMemoryWorkOrderRepository>();
    builder.Services.AddSingleton<ISerialUnitRepository, InMemorySerialUnitRepository>();
    builder.Services.AddSingleton<ITestRecordRepository, InMemoryTestRecordRepository>();
    builder.Services.AddSingleton<ITraceEventRepository, InMemoryTraceEventRepository>();
    builder.Services.AddSingleton<IStationRepository, InMemoryStationRepository>();
    builder.Services.AddSingleton<ITestFlowRepository, InMemoryTestFlowRepository>();
    builder.Services.AddSingleton<ISpcRuleRepository, InMemorySpcRuleRepository>();
    builder.Services.AddSingleton<IAlarmEventRepository, InMemoryAlarmEventRepository>();
    builder.Services.AddSingleton<IMesExecutionService, MesExecutionService>();
    builder.Services.AddSingleton<IStationService, StationService>();
    builder.Services.AddSingleton<ITestFlowService, TestFlowService>();
    builder.Services.AddSingleton<ISpcService, SpcService>();
}
else
{
    builder.Services.AddDbContext<MesDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("MesDb")));

    builder.Services.AddScoped<IWorkOrderRepository, PostgresWorkOrderRepository>();
    builder.Services.AddScoped<ISerialUnitRepository, PostgresSerialUnitRepository>();
    builder.Services.AddScoped<ITestRecordRepository, PostgresTestRecordRepository>();
    builder.Services.AddScoped<ITraceEventRepository, PostgresTraceEventRepository>();
    builder.Services.AddScoped<IStationRepository, PostgresStationRepository>();
    builder.Services.AddScoped<ITestFlowRepository, PostgresTestFlowRepository>();
    builder.Services.AddScoped<ISpcRuleRepository, PostgresSpcRuleRepository>();
    builder.Services.AddScoped<IAlarmEventRepository, PostgresAlarmEventRepository>();
    builder.Services.AddScoped<IMesExecutionService, MesExecutionService>();
    builder.Services.AddScoped<IStationService, StationService>();
    builder.Services.AddScoped<ITestFlowService, TestFlowService>();
    builder.Services.AddScoped<ISpcService, SpcService>();
}

var app = builder.Build();

if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MesDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

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

app.MapPost("/api/test-results", async (
    UploadTestResultRequest request,
    IMesExecutionService service,
    ISpcService spcService,
    IHubContext<DashboardHub> dashboardHub) =>
{
    var result = await service.UploadTestResultAsync(request);

    if (result.Success)
    {
        var realtime = await spcService.GetRealtimeAsync(null, request.StationCode);
        await dashboardHub.Clients.All.SendAsync(DashboardHub.DashboardUpdatedEvent, realtime);
    }

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

app.MapGet("/api/spc/summary", async (
    string? productCode,
    string? stationCode,
    DateTimeOffset? from,
    DateTimeOffset? to,
    ISpcService service) =>
{
    var result = await service.GetSummaryAsync(productCode, stationCode, from, to);
    return Results.Ok(result);
})
.WithName("GetSpcSummary")
.WithOpenApi();

app.MapGet("/api/spc/rules", async (string? productCode, string? stationCode, ISpcService service) =>
{
    var result = await service.GetRulesAsync(productCode, stationCode);
    return Results.Ok(result);
})
.WithName("GetSpcRules")
.WithOpenApi();

app.MapPost("/api/spc/rules", async (CreateSpcRuleRequest request, ISpcService service) =>
{
    var result = await service.CreateRuleAsync(request);
    return Results.Ok(result);
})
.WithName("CreateSpcRule")
.WithOpenApi();

app.MapPut("/api/spc/rules/{ruleCode}", async (string ruleCode, UpdateSpcRuleRequest request, ISpcService service) =>
{
    var result = await service.UpdateRuleAsync(ruleCode, request);
    return Results.Ok(result);
})
.WithName("UpdateSpcRule")
.WithOpenApi();

app.MapDelete("/api/spc/rules/{ruleCode}", async (string ruleCode, ISpcService service) =>
{
    var result = await service.DeleteRuleAsync(ruleCode);
    return Results.Ok(result);
})
.WithName("DeleteSpcRule")
.WithOpenApi();

app.MapGet("/api/dashboard/realtime", async (string? productCode, string? stationCode, ISpcService service) =>
{
    var result = await service.GetRealtimeAsync(productCode, stationCode);
    return Results.Ok(result);
})
.WithName("GetDashboardRealtime")
.WithOpenApi();

app.MapHub<DashboardHub>(DashboardHub.HubPath);

app.Run();
