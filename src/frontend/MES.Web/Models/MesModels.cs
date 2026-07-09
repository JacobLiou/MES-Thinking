namespace MES.Web.Models;

public sealed class CommandResult
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class CreateWorkOrderRequest
{
    public string WorkOrderNo { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int PlannedQty { get; set; }
}

public sealed class StationPassRequest
{
    public string Sn { get; set; } = string.Empty;
    public string WorkOrderNo { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
}

public sealed class UploadTestResultRequest
{
    public string Sn { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string TestBatchId { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public Dictionary<string, double>? Metrics { get; set; }
    public string? RawPayload { get; set; }
}

public sealed class StationResponse
{
    public string StationCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LineCode { get; set; } = string.Empty;
    public bool IsTestStation { get; set; }
}

public sealed class RouteStepResponse
{
    public int Sequence { get; set; }
    public string StepCode { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public bool AllowRework { get; set; }
}

public sealed class TestFlowResponse
{
    public string FlowCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<RouteStepResponse> Steps { get; set; } = [];
}

public sealed class TraceabilityResponse
{
    public string Sn { get; set; } = string.Empty;
    public string? WorkOrderNo { get; set; }
    public string? CurrentStationCode { get; set; }
    public string? CurrentStatus { get; set; }
    public string? ActiveFlowCode { get; set; }
    public int? CompletedStepSequence { get; set; }
    public string? NextExpectedStation { get; set; }
    public List<TraceTimelineItem> Timeline { get; set; } = [];
    public List<TestResultItem> TestResults { get; set; } = [];
}

public sealed class TraceTimelineItem
{
    public string EventType { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class TestResultItem
{
    public string StationCode { get; set; } = string.Empty;
    public string TestBatchId { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = [];
    public DateTimeOffset TestedAt { get; set; }
}

public sealed class OperationLogEntry
{
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.Now;
    public string Action { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public sealed class CreateSpcRuleRequest
{
    public string RuleCode { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? StationCode { get; set; }
    public double? LowerLimit { get; set; }
    public double? UpperLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateSpcRuleRequest
{
    public string MetricName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? StationCode { get; set; }
    public double? LowerLimit { get; set; }
    public double? UpperLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class SpcSummaryResponse
{
    public DateTimeOffset WindowStart { get; set; }
    public DateTimeOffset WindowEnd { get; set; }
    public string? ProductCode { get; set; }
    public string? StationCode { get; set; }
    public int SampleCount { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public double YieldRate { get; set; }
    public double FirstPassYieldRate { get; set; }
    public List<SpcMetricSummaryItem> Metrics { get; set; } = [];
}

public sealed class SpcMetricSummaryItem
{
    public string MetricName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Mean { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
}

public sealed class SpcRuleResponse
{
    public string RuleCode { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? StationCode { get; set; }
    public double? LowerLimit { get; set; }
    public double? UpperLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AlarmEventResponse
{
    public string AlarmCode { get; set; } = string.Empty;
    public string Sn { get; set; } = string.Empty;
    public string StationCode { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double MetricValue { get; set; }
    public double? LowerLimit { get; set; }
    public double? UpperLimit { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class DashboardRealtimeResponse
{
    public SpcSummaryResponse Summary { get; set; } = new();
    public List<AlarmEventResponse> LatestAlarms { get; set; } = [];
}
