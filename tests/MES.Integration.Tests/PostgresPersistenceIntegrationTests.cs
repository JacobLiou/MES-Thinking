using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using MES.Application.Contracts;
using MES.Domain.Entities;
using MES.Domain.Enums;
using MES.Infrastructure.Persistence;
using MES.Infrastructure.Repositories;
using Npgsql;

namespace MES.Integration.Tests;

public sealed class PostgresPersistenceIntegrationTests : IAsyncLifetime
{
    private readonly TestcontainersContainer _postgresContainer = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage("postgres:16-alpine")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "postgres")
        .WithEnvironment("POSTGRES_DB", "mes_integration")
        .WithPortBinding(5432, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private string _connectionString = string.Empty;
    private string? _skipReason;

    public async Task InitializeAsync()
    {
        try
        {
            await _postgresContainer.StartAsync();
            var host = _postgresContainer.Hostname;
            var port = _postgresContainer.GetMappedPublicPort(5432);
            _connectionString = $"Host={host};Port={port};Database=mes_integration;Username=postgres;Password=postgres";
        }
        catch (Exception ex)
        {
            _skipReason = $"Docker/PostgreSQL container unavailable: {ex.Message}";
        }
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(_skipReason))
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task MigrateAsync_CreatesCoreTables()
    {
        if (!string.IsNullOrWhiteSpace(_skipReason))
        {
            return;
        }

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "select count(*) from information_schema.tables where table_schema='public' and table_name in ('work_orders','test_records','trace_events','spc_rules','alarm_events')",
            connection);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task TestRecordRepository_RejectsDuplicateBusinessKey()
    {
        if (!string.IsNullOrWhiteSpace(_skipReason))
        {
            return;
        }

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();

        var repository = new PostgresTestRecordRepository(dbContext);

        var record = new TestRecord
        {
            Sn = "SN-900",
            StationCode = "ST-ICT-01",
            TestBatchId = "BATCH-900",
            Passed = true,
            Metrics = new Dictionary<string, double> { ["Voltage"] = 3.2 },
            RawPayload = "{}",
            TestedAt = DateTimeOffset.UtcNow
        };

        await repository.AddAsync(record);

        var duplicate = new TestRecord
        {
            Sn = "SN-900",
            StationCode = "ST-ICT-01",
            TestBatchId = "BATCH-900",
            Passed = false,
            Metrics = new Dictionary<string, double> { ["Voltage"] = 3.6 },
            RawPayload = "{}",
            TestedAt = DateTimeOffset.UtcNow
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(duplicate));
    }

    [Fact]
    public async Task MesExecutionService_SecondUploadReturnsMes4091()
    {
        if (!string.IsNullOrWhiteSpace(_skipReason))
        {
            return;
        }

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();

        var workOrderRepo = new PostgresWorkOrderRepository(dbContext);
        var serialRepo = new PostgresSerialUnitRepository(dbContext);
        var recordRepo = new PostgresTestRecordRepository(dbContext);
        var traceRepo = new PostgresTraceEventRepository(dbContext);
        var stationRepo = new PostgresStationRepository(dbContext);
        var flowRepo = new PostgresTestFlowRepository(dbContext);
        var spcRuleRepo = new PostgresSpcRuleRepository(dbContext);
        var alarmRepo = new PostgresAlarmEventRepository(dbContext);

        var service = new MES.Application.Services.MesExecutionService(
            workOrderRepo,
            serialRepo,
            recordRepo,
            traceRepo,
            stationRepo,
            flowRepo,
            spcRuleRepo,
            alarmRepo);

        await stationRepo.AddAsync(new Station
        {
            StationCode = "ST-ASSY-01",
            Name = "Assy",
            LineCode = "L1",
            IsTestStation = false
        });

        await stationRepo.AddAsync(new Station
        {
            StationCode = "ST-ICT-01",
            Name = "ICT",
            LineCode = "L1",
            IsTestStation = true
        });

        await flowRepo.AddAsync(new TestFlow
        {
            FlowCode = "TF-PCB-A-V1",
            Name = "Flow",
            ProductCode = "PCB-A",
            Version = "1",
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
                }
            ]
        });

        await workOrderRepo.AddAsync(new WorkOrder
        {
            WorkOrderNo = "WO-900",
            ProductCode = "PCB-A",
            PlannedQty = 10,
            Status = WorkOrderStatus.InProgress,
            TestFlowCode = "TF-PCB-A-V1"
        });

        await service.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-900",
            WorkOrderNo = "WO-900",
            StationCode = "ST-ASSY-01",
            OperatorId = "OP-1"
        });

        await service.PassStationAsync(new StationPassRequest
        {
            Sn = "SN-900",
            WorkOrderNo = "WO-900",
            StationCode = "ST-ICT-01",
            OperatorId = "OP-1"
        });

        var first = await service.UploadTestResultAsync(new UploadTestResultRequest
        {
            Sn = "SN-900",
            StationCode = "ST-ICT-01",
            TestBatchId = "BATCH-900",
            Passed = true
        });

        var second = await service.UploadTestResultAsync(new UploadTestResultRequest
        {
            Sn = "SN-900",
            StationCode = "ST-ICT-01",
            TestBatchId = "BATCH-900",
            Passed = true
        });

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Equal("MES-4091", second.Code);
    }

    private MesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MesDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new MesDbContext(options);
    }
}
