using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Hunter.Client.Services;

public sealed class RealtimeService : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private HubConnection? _connection;
    public HubConnection? Connection => _connection;

    public RealtimeService(IConfiguration config)
    {
        _config = config;
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_connection is not null && _connection.State != HubConnectionState.Disconnected)
            return;

        // Read base URL and hub path from config
        var apiBase = _config["ApiBaseUrl"];              // or "ObserverServer:BaseUrl" if that's what you're using
        if (string.IsNullOrWhiteSpace(apiBase))
            apiBase = "https://localhost:7273";

        var hubPath = _config["SignalR:ClientHubPath"];   // e.g. "/hubs/observer"
        if (string.IsNullOrWhiteSpace(hubPath))
            hubPath = "/hubs/observer";

        var hubUrl = $"{apiBase}{hubPath}";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)                              // <---- USE hubUrl, NOT /hubs/client
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync(ct);
    }


    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            try { await _connection.StopAsync(); } catch { }
            try { await _connection.DisposeAsync(); } catch { }
        }
    }
}
