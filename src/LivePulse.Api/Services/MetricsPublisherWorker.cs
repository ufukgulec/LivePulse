using Microsoft.AspNetCore.SignalR;
using LivePulse.Api.Hubs;

namespace LivePulse.Api.Services;

public class MetricsPublisherWorker : BackgroundService
{
    private readonly IHubContext<MetricsHub, IMetricsClient> _hubContext;
    private readonly ILogger<MetricsPublisherWorker> _logger;
    private readonly string _nodeName = Environment.GetEnvironmentVariable("NODE_NAME") ?? "Node-Primary";

    public MetricsPublisherWorker(
        IHubContext<MetricsHub, IMetricsClient> hubContext,
        ILogger<MetricsPublisherWorker> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            var cpuUsage = Math.Round(random.NextDouble() * 100, 1);
            var ramUsage = Math.Round(45 + random.NextDouble() * 50, 1);
            var now = DateTime.UtcNow;

            // Tüm bağlı istemcilere canlı yayın yap.
            await _hubContext.Clients.All.ReceiveMetrics(_nodeName, cpuUsage, ramUsage, now);

            // CPU %85 üzerine çıkarsa kritik uyarı gönder.
            if (cpuUsage > 85)
            {
                await _hubContext.Clients.All.ReceiveSystemAlert(
                    $"[{_nodeName}] High CPU load detected: %{cpuUsage}", "WARNING");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
