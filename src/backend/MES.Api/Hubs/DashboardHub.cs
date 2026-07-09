using Microsoft.AspNetCore.SignalR;

namespace MES.Api.Hubs;

public sealed class DashboardHub : Hub
{
    public const string HubPath = "/hubs/dashboard";
    public const string DashboardUpdatedEvent = "dashboard.updated";
}
