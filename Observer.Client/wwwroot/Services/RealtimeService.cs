// Observer.Client/Services/RealtimeService.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Observer.Client.Services;

public sealed class RealtimeService : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private HubConnection? _connection;

    public RealtimeService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// The underlying HubConnection, if you need to wire up handlers later.
    /// </summary>
    public HubConnection? Connection => _connection;

    /// <summary>
    /// True when connected to the Hunter SignalR hub.
    /// </summary>
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    /// <summary>
    /// True while a connect operation is in progress.
    /// </summary>
    public bool IsConnecting { get; private set; }

    /// <summary>
    /// Base URL of the Hunter server we are currently connected to, if any.
    /// </summary>
    public string? CurrentServerBaseUrl { get; private set; }

    /// <summary>
    /// Connect to the Hunter server hub at the given base URL.
    /// The hub path is read from config: SignalR:HunterHubPath (defaults to /hubs/hunter).
    /// </summary>
    public async Task ConnectAsync(string hunterBaseUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(hunterBaseUrl))
        {
            throw new ArgumentException("Hunter base URL is required.", nameof(hunterBaseUrl));
        }

        // If we're already connected to this same server, do nothing.
        if (IsConnected &&
            CurrentServerBaseUrl is not null &&
            string.Equals(CurrentServerBaseUrl, hunterBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Tear down any existing connection first.
        if (_connection is not null)
        {
            try { await _connection.StopAsync(ct); }
            catch { /* swallow */ }

            try { await _connection.DisposeAsync(); }
            catch { /* swallow */ }

            _connection = null;
            CurrentServerBaseUrl = null;
        }

        var hubPath = _config["SignalR:HunterHubPath"];
        if (string.IsNullOrWhiteSpace(hubPath))
        {
            hubPath = "/hubs/hunter";
        }

        // Make sure the hub path starts with "/"
        if (!hubPath.StartsWith("/", StringComparison.Ordinal))
        {
            hubPath = "/" + hubPath;
        }

        // Do NOT add a trailing slash to hunterBaseUrl; just concatenate.
        var hubUrl = $"{hunterBaseUrl}{hubPath}";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        IsConnecting = true;
        CurrentServerBaseUrl = hunterBaseUrl;

        try
        {
            await _connection.StartAsync(ct);
        }
        finally
        {
            IsConnecting = false;
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_connection is null)
        {
            return;
        }

        try { await _connection.StopAsync(ct); }
        catch { /* swallow */ }

        try { await _connection.DisposeAsync(); }
        catch { /* swallow */ }

        _connection = null;
        CurrentServerBaseUrl = null;
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
