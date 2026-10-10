using LivePulse.Api.Hubs;
using LivePulse.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// SignalR & Redis Backplane
var signalRBuilder = builder.Services.AddSignalR();
var redisConn = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrEmpty(redisConn))
{
    signalRBuilder.AddStackExchangeRedis(redisConn, options =>
    {
        options.Configuration.ChannelPrefix = "LivePulse";
    });
}

builder.Services.AddHostedService<MetricsPublisherWorker>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<MetricsHub>("/hubs/metrics");

app.Run();