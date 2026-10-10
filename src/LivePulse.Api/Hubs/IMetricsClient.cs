namespace LivePulse.Api.Hubs;

public interface IMetricsClient
{
    Task ReceiveMetrics(string nodeName, double cpuUsage, double ramUsage, DateTime timestamp);
    Task ReceiveSystemAlert(string message, string level);
}