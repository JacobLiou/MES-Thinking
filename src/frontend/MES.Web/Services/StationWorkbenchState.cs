using MES.Web.Models;

namespace MES.Web.Services;

public sealed class StationWorkbenchState
{
    public string SelectedStationCode { get; set; } = string.Empty;
    public string WorkOrderNo { get; set; } = "WO-DEMO-001";
    public string Sn { get; set; } = string.Empty;
    public string OperatorId { get; set; } = "OP-001";
    public string TestBatchId { get; set; } = string.Empty;
    public List<OperationLogEntry> OperationLogs { get; } = [];

    public void AddLog(string action, CommandResult result)
    {
        OperationLogs.Insert(0, new OperationLogEntry
        {
            Action = action,
            Code = result.Code,
            Message = result.Message,
            Success = result.Success
        });

        if (OperationLogs.Count > 20)
        {
            OperationLogs.RemoveAt(OperationLogs.Count - 1);
        }
    }

    public void RefreshTestBatchId() =>
        TestBatchId = $"B-{DateTimeOffset.Now:yyyyMMddHHmmss}";
}
