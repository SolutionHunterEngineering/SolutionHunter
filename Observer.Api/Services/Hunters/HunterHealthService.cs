// ========= Services\Hunters\HunterHealthService.cs =========
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Observer.Api.Options;
using Observer.API.Database;
using Observer.Shared.DTOs.Hunters;

namespace Observer.Api.Services.Hunters;

public sealed class HunterHealthService
{
    private readonly HunterServersOptions _servers;
    private readonly HunterConnectionManager _connManager;
    private readonly IDbContextFactory<DataContext> _dbFactory;

    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(2);

    public HunterHealthService(
        IOptions<HunterServersOptions> servers,
        HunterConnectionManager connManager,
        IDbContextFactory<DataContext> dbFactory)
    {
        _servers = servers.Value;
        _connManager = connManager;
        _dbFactory = dbFactory;
    }

    public async Task<List<HunterHealthDto>> GetStatusesAsync(TimeSpan ttl)
    {
        await using var db = _dbFactory.CreateDbContext();
        var now = DateTime.UtcNow;

        var rows = new List<HunterServerRecord>();
        foreach (var s in _servers.HunterServers)
        {
            var row = await db.HunterServers.FindAsync(s.Id);
            if (row is null)
            {
                row = new HunterServerRecord
                {
                    Id = s.Id,
                    Name = s.Name,
                    BaseUrl = s.BaseUrl,
                    Description = "",
                    Kind = "",
                    IsRunning = false,
                    WorkCapacity = 0,
                    CurrentWorkLoad = 0,
                    LastCheckedUtc = DateTime.MinValue,
                    LatencyMs = null,
                    Error = "Unknown"
                };
                db.HunterServers.Add(row);
            }
            else
            {
                // Keep Name/BaseUrl in sync with config
                row.Name = s.Name;
                row.BaseUrl = s.BaseUrl;
            }
            rows.Add(row);
        }

        // Refresh rows that are stale (pings run in parallel; rows are tracked objects, no DB calls inside)
        var stale = ttl <= TimeSpan.Zero
            ? rows
            : rows.Where(r => now - r.LastCheckedUtc >= ttl).ToList();

        if (stale.Count > 0)
            await Task.WhenAll(stale.Select(RefreshOneAsync));

        await db.SaveChangesAsync();

        return rows.Select(ToDto).ToList();
    }

    private async Task RefreshOneAsync(HunterServerRecord row)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var pingTask = _connManager.PingAsync(row.Id, "ping");
            var completed = await Task.WhenAny(pingTask, Task.Delay(PingTimeout));
            sw.Stop();

            row.LastCheckedUtc = DateTime.UtcNow;
            row.LatencyMs = (int)sw.ElapsedMilliseconds;

            if (completed != pingTask)
            {
                row.IsRunning = false;
                row.Error = "Ping timeout";
                return;
            }

            var reply = await pingTask;
            row.IsRunning = !string.IsNullOrWhiteSpace(reply);
            row.Error = row.IsRunning ? null : "Empty ping reply";
        }
        catch (Exception ex)
        {
            sw.Stop();
            row.LastCheckedUtc = DateTime.UtcNow;
            row.LatencyMs = (int)sw.ElapsedMilliseconds;
            row.IsRunning = false;
            row.Error = ex.Message;
        }
    }

    private static HunterHealthDto ToDto(HunterServerRecord row) => new()
    {
        ServerId = row.Id,
        Name = row.Name,
        Description = row.Description,
        BaseUrl = row.BaseUrl,
        Kind = row.Kind,
        IsRunning = row.IsRunning,
        WorkCapacity = row.WorkCapacity,
        CurrentWorkLoad = row.CurrentWorkLoad,
        LastCheckedUtc = row.LastCheckedUtc == DateTime.MinValue ? null : row.LastCheckedUtc,
        LatencyMs = row.LatencyMs,
        Error = row.Error
    };
}
