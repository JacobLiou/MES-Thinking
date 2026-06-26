using MES.Application.Contracts;
using MES.Application.Services;
using MES.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IWorkOrderRepository, InMemoryWorkOrderRepository>();
builder.Services.AddSingleton<ISerialUnitRepository, InMemorySerialUnitRepository>();
builder.Services.AddSingleton<ITestRecordRepository, InMemoryTestRecordRepository>();
builder.Services.AddSingleton<ITraceEventRepository, InMemoryTraceEventRepository>();
builder.Services.AddSingleton<IMesExecutionService, MesExecutionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/work-orders", async (CreateWorkOrderRequest request, IMesExecutionService service) =>
{
    var result = await service.CreateWorkOrderAsync(request);
    return Results.Ok(result);
})
.WithName("CreateWorkOrder")
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
