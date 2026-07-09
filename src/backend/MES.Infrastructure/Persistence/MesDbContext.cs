using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MES.Domain.Entities;
using MES.Domain.Enums;

namespace MES.Infrastructure.Persistence;

public sealed class MesDbContext : DbContext
{
    public MesDbContext(DbContextOptions<MesDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<SerialUnit> SerialUnits => Set<SerialUnit>();
    public DbSet<TestRecord> TestRecords => Set<TestRecord>();
    public DbSet<TraceEvent> TraceEvents => Set<TraceEvent>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<TestFlow> TestFlows => Set<TestFlow>();
    public DbSet<RouteStep> RouteSteps => Set<RouteStep>();
    public DbSet<SpcRule> SpcRules => Set<SpcRule>();
    public DbSet<AlarmEvent> AlarmEvents => Set<AlarmEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureWorkOrders(modelBuilder);
        ConfigureSerialUnits(modelBuilder);
        ConfigureStations(modelBuilder);
        ConfigureTestFlows(modelBuilder);
        ConfigureRouteSteps(modelBuilder);
        ConfigureTestRecords(modelBuilder);
        ConfigureTraceEvents(modelBuilder);
        ConfigureSpcRules(modelBuilder);
        ConfigureAlarmEvents(modelBuilder);
    }

    private static void ConfigureWorkOrders(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<WorkOrder>();
        builder.ToTable("work_orders");
        builder.HasKey(x => x.WorkOrderNo);
        builder.Property(x => x.WorkOrderNo).HasColumnName("work_order_no");
        builder.Property(x => x.ProductCode).HasColumnName("product_code");
        builder.Property(x => x.PlannedQty).HasColumnName("planned_qty");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>();
        builder.Property(x => x.TestFlowCode).HasColumnName("test_flow_code");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.ProductCode);
    }

    private static void ConfigureSerialUnits(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<SerialUnit>();
        builder.ToTable("serial_units");
        builder.HasKey(x => x.Sn);
        builder.Property(x => x.Sn).HasColumnName("sn");
        builder.Property(x => x.WorkOrderNo).HasColumnName("work_order_no");
        builder.Property(x => x.CurrentStationCode).HasColumnName("current_station_code");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>();
        builder.Property(x => x.LastTestPassed).HasColumnName("last_test_passed");
        builder.Property(x => x.CompletedStepSequence).HasColumnName("completed_step_sequence");
        builder.Property(x => x.PendingStepSequence).HasColumnName("pending_step_sequence");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.HasIndex(x => x.WorkOrderNo);
    }

    private static void ConfigureStations(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Station>();
        builder.ToTable("stations");
        builder.HasKey(x => x.StationCode);
        builder.Property(x => x.StationCode).HasColumnName("station_code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.LineCode).HasColumnName("line_code");
        builder.Property(x => x.IsTestStation).HasColumnName("is_test_station");
    }

    private static void ConfigureTestFlows(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TestFlow>();
        builder.ToTable("test_flows");
        builder.HasKey(x => x.FlowCode);
        builder.Property(x => x.FlowCode).HasColumnName("flow_code");
        builder.Property(x => x.Name).HasColumnName("name");
        builder.Property(x => x.ProductCode).HasColumnName("product_code");
        builder.Property(x => x.Version).HasColumnName("version");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Ignore(x => x.Steps);
        builder.HasIndex(x => x.ProductCode);
    }

    private static void ConfigureRouteSteps(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<RouteStep>();
        builder.ToTable("route_steps");
        builder.HasKey(x => new { x.FlowCode, x.Sequence });
        builder.Property(x => x.FlowCode).HasColumnName("flow_code");
        builder.Property(x => x.Sequence).HasColumnName("sequence");
        builder.Property(x => x.StepCode).HasColumnName("step_code");
        builder.Property(x => x.StationCode).HasColumnName("station_code");
        builder.Property(x => x.StepType)
            .HasColumnName("step_type")
            .HasConversion<string>();
        builder.Property(x => x.AllowRework).HasColumnName("allow_rework");
        builder.HasIndex(x => x.StationCode);
    }

    private static void ConfigureTestRecords(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TestRecord>();
        builder.ToTable("test_records");
        builder.HasKey(x => new { x.Sn, x.StationCode, x.TestBatchId });
        builder.Property(x => x.Sn).HasColumnName("sn");
        builder.Property(x => x.StationCode).HasColumnName("station_code");
        builder.Property(x => x.TestBatchId).HasColumnName("test_batch_id");
        builder.Property(x => x.Passed).HasColumnName("passed");
        builder.Property(x => x.RawPayload).HasColumnName("raw_payload");
        builder.Property(x => x.TestedAt).HasColumnName("tested_at");

        var metricsConverter = new ValueConverter<Dictionary<string, double>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, double>());

        var metricsComparer = new ValueComparer<Dictionary<string, double>>(
            (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null).GetHashCode(),
            value => JsonSerializer.Deserialize<Dictionary<string, double>>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new Dictionary<string, double>());

        builder.Property(x => x.Metrics)
            .HasColumnName("metrics")
            .HasColumnType("jsonb")
            .HasConversion(metricsConverter)
            .Metadata.SetValueComparer(metricsComparer);

        builder.HasIndex(x => new { x.Sn, x.StationCode, x.TestBatchId })
            .IsUnique();
        builder.HasIndex(x => x.TestedAt);
    }

    private static void ConfigureTraceEvents(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TraceEvent>();
        builder.ToTable("trace_events");
        builder.HasKey(x => new { x.Sn, x.OccurredAt, x.EventType, x.StationCode, x.OperatorId });
        builder.Property(x => x.Sn).HasColumnName("sn");
        builder.Property(x => x.EventType).HasColumnName("event_type");
        builder.Property(x => x.StationCode).HasColumnName("station_code");
        builder.Property(x => x.OperatorId).HasColumnName("operator_id");
        builder.Property(x => x.Message).HasColumnName("message");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.HasIndex(x => new { x.Sn, x.OccurredAt });
    }

    private static void ConfigureSpcRules(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<SpcRule>();
        builder.ToTable("spc_rules");
        builder.HasKey(x => x.RuleCode);
        builder.Property(x => x.RuleCode).HasColumnName("rule_code");
        builder.Property(x => x.MetricName).HasColumnName("metric_name");
        builder.Property(x => x.ProductCode).HasColumnName("product_code");
        builder.Property(x => x.StationCode).HasColumnName("station_code");
        builder.Property(x => x.LowerLimit).HasColumnName("lower_limit");
        builder.Property(x => x.UpperLimit).HasColumnName("upper_limit");
        builder.Property(x => x.IsActive).HasColumnName("is_active");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => new { x.ProductCode, x.StationCode, x.MetricName });
    }

    private static void ConfigureAlarmEvents(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<AlarmEvent>();
        builder.ToTable("alarm_events");
        builder.HasKey(x => x.AlarmCode);
        builder.Property(x => x.AlarmCode).HasColumnName("alarm_code");
        builder.Property(x => x.Sn).HasColumnName("sn");
        builder.Property(x => x.StationCode).HasColumnName("station_code");
        builder.Property(x => x.MetricName).HasColumnName("metric_name");
        builder.Property(x => x.MetricValue).HasColumnName("metric_value");
        builder.Property(x => x.LowerLimit).HasColumnName("lower_limit");
        builder.Property(x => x.UpperLimit).HasColumnName("upper_limit");
        builder.Property(x => x.Severity).HasColumnName("severity");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.Message).HasColumnName("message");
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at");
        builder.HasIndex(x => new { x.StationCode, x.OccurredAt });
    }
}
