using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alarm_events",
                columns: table => new
                {
                    alarm_code = table.Column<string>(type: "text", nullable: false),
                    sn = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    metric_name = table.Column<string>(type: "text", nullable: false),
                    metric_value = table.Column<double>(type: "double precision", nullable: false),
                    lower_limit = table.Column<double>(type: "double precision", nullable: true),
                    upper_limit = table.Column<double>(type: "double precision", nullable: true),
                    severity = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarm_events", x => x.alarm_code);
                });

            migrationBuilder.CreateTable(
                name: "route_steps",
                columns: table => new
                {
                    flow_code = table.Column<string>(type: "text", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    step_code = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    step_type = table.Column<string>(type: "text", nullable: false),
                    allow_rework = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_steps", x => new { x.flow_code, x.sequence });
                });

            migrationBuilder.CreateTable(
                name: "serial_units",
                columns: table => new
                {
                    sn = table.Column<string>(type: "text", nullable: false),
                    work_order_no = table.Column<string>(type: "text", nullable: false),
                    current_station_code = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    last_test_passed = table.Column<bool>(type: "boolean", nullable: true),
                    completed_step_sequence = table.Column<int>(type: "integer", nullable: true),
                    pending_step_sequence = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_serial_units", x => x.sn);
                });

            migrationBuilder.CreateTable(
                name: "spc_rules",
                columns: table => new
                {
                    rule_code = table.Column<string>(type: "text", nullable: false),
                    metric_name = table.Column<string>(type: "text", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: true),
                    station_code = table.Column<string>(type: "text", nullable: true),
                    lower_limit = table.Column<double>(type: "double precision", nullable: true),
                    upper_limit = table.Column<double>(type: "double precision", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spc_rules", x => x.rule_code);
                });

            migrationBuilder.CreateTable(
                name: "stations",
                columns: table => new
                {
                    station_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    line_code = table.Column<string>(type: "text", nullable: false),
                    is_test_station = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stations", x => x.station_code);
                });

            migrationBuilder.CreateTable(
                name: "test_flows",
                columns: table => new
                {
                    flow_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_flows", x => x.flow_code);
                });

            migrationBuilder.CreateTable(
                name: "test_records",
                columns: table => new
                {
                    sn = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    test_batch_id = table.Column<string>(type: "text", nullable: false),
                    passed = table.Column<bool>(type: "boolean", nullable: false),
                    metrics = table.Column<string>(type: "jsonb", nullable: false),
                    raw_payload = table.Column<string>(type: "text", nullable: true),
                    tested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_records", x => new { x.sn, x.station_code, x.test_batch_id });
                });

            migrationBuilder.CreateTable(
                name: "trace_events",
                columns: table => new
                {
                    sn = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    station_code = table.Column<string>(type: "text", nullable: false),
                    operator_id = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trace_events", x => new { x.sn, x.occurred_at, x.event_type, x.station_code, x.operator_id });
                });

            migrationBuilder.CreateTable(
                name: "work_orders",
                columns: table => new
                {
                    work_order_no = table.Column<string>(type: "text", nullable: false),
                    product_code = table.Column<string>(type: "text", nullable: false),
                    planned_qty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    test_flow_code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_orders", x => x.work_order_no);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alarm_events_station_code_occurred_at",
                table: "alarm_events",
                columns: new[] { "station_code", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_route_steps_station_code",
                table: "route_steps",
                column: "station_code");

            migrationBuilder.CreateIndex(
                name: "IX_serial_units_work_order_no",
                table: "serial_units",
                column: "work_order_no");

            migrationBuilder.CreateIndex(
                name: "IX_spc_rules_product_code_station_code_metric_name",
                table: "spc_rules",
                columns: new[] { "product_code", "station_code", "metric_name" });

            migrationBuilder.CreateIndex(
                name: "IX_test_flows_product_code",
                table: "test_flows",
                column: "product_code");

            migrationBuilder.CreateIndex(
                name: "IX_test_records_sn_station_code_test_batch_id",
                table: "test_records",
                columns: new[] { "sn", "station_code", "test_batch_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_test_records_tested_at",
                table: "test_records",
                column: "tested_at");

            migrationBuilder.CreateIndex(
                name: "IX_trace_events_sn_occurred_at",
                table: "trace_events",
                columns: new[] { "sn", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_product_code",
                table: "work_orders",
                column: "product_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alarm_events");

            migrationBuilder.DropTable(
                name: "route_steps");

            migrationBuilder.DropTable(
                name: "serial_units");

            migrationBuilder.DropTable(
                name: "spc_rules");

            migrationBuilder.DropTable(
                name: "stations");

            migrationBuilder.DropTable(
                name: "test_flows");

            migrationBuilder.DropTable(
                name: "test_records");

            migrationBuilder.DropTable(
                name: "trace_events");

            migrationBuilder.DropTable(
                name: "work_orders");
        }
    }
}
