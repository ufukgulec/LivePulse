using Microsoft.AspNetCore.SignalR;

namespace LivePulse.Api.Hubs;

public class MetricsHub : Hub<IMetricsClient>
{
    private readonly ILogger<MetricsHub> _logger;

    public MetricsHub(ILogger<MetricsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task JoinServerGroup(string serverName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, serverName);
        await Clients.Caller.ReceiveSystemAlert($"Subscribed to updates for: {serverName}", "INFO");
    }

    public async Task LeaveServerGroup(string serverName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, serverName);
    }
}