// ========= Services\Hunters\HunterHealthService.cs =========
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Observer.Api.Options;
using Observer.Shared.DTOs.Hunters;

namespace Observer.Api.Services.Hunters;

public sealed class HunterHealthService
{
    private readonly HunterServersOptions _servers;
    private readonly HunterConnectionManager _connManager;

    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(2);

    // In-memory cache for now (SQL comes next)
    private readonly Dictionary<string, HunterHealthDto> _cache = new();
    private DateTime _lastSweepUtc = DateTime.MinValue;

    public HunterHealthService(
        IOptions<HunterServersOptions> servers,
        HunterConnectionManager connManager)
    {
        _servers = servers.Value;
        _connManager = connManager;
    }

    public async Task<List<HunterHealthDto>> GetStatusesAsync(TimeSpan ttl)
    {
        // Always return something immediately (cached or unknown)
        var list = _servers.HunterServers.Select(s => GetOrCreateRow(s)).ToList();

        // If stale, refresh now
        var now = DateTime.UtcNow;
        if (now - _lastSweepUtc >= ttl)
        {
            _lastSweepUtc = now;
            await RefreshAllAsync(list);
        }

        return list;
    }

    private HunterHealthDto GetOrCreateRow(HunterServer s)
    {
        if (_cache.TryGetValue(s.Id, out var row))
        {
            row.Name = s.Name;
            row.BaseUrl = s.BaseUrl;
            return row;
        }

        row = new HunterHealthDto
        {
            ServerId = s.Id,
            Name = s.Name,
            BaseUrl = s.BaseUrl,
            IsHealthy = false,
            LastCheckedUtc = null,
            LatencyMs = null,
            Error = "Unknown"
        };

        _cache[s.Id] = row;
        return row;
    }

    private async Task RefreshAllAsync(List<HunterHealthDto> rows)
    {
        var tasks = rows.Select(RefreshOneAsync).ToArray();
        await Task.WhenAll(tasks);
    }

    private async Task RefreshOneAsync(HunterHealthDto row)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            // Ping over SignalR to the Hunter observer-bridge
            // (Observer API authenticates to Hunter using the configured ServiceUser/ServicePassword
            // inside HunterConnectionManager when it creates the connection)
            var pingTask = _connManager.PingAsync(row.ServerId, "ping");

            var completed = await Task.WhenAny(pingTask, Task.Delay(PingTimeout));
            sw.Stop();

            row.LastCheckedUtc = DateTime.UtcNow;
            row.LatencyMs = (int)sw.ElapsedMilliseconds;

            if (completed != pingTask)
            {
                row.IsHealthy = false;
                row.Error = "Ping timeout";
                return;
            }

            var reply = await pingTask; // already completed
            row.IsHealthy = !string.IsNullOrWhiteSpace(reply);
            row.Error = row.IsHealthy ? null : "Empty ping reply";
        }
        catch (Exception ex)
        {
            sw.Stop();
            row.LastCheckedUtc = DateTime.UtcNow;
            row.LatencyMs = (int)sw.ElapsedMilliseconds;
            row.IsHealthy = false;
            row.Error = ex.Message;
        }
    }
}
