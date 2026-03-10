using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Observer.Api.Options;

namespace Observer.Api.Services.Hunters;

public sealed class HunterConnectionManager
{
    private readonly IOptions<HunterServersOptions> _servers;
    private readonly ConcurrentDictionary<string, Lazy<Task<HunterConn>>> _conns = new();

    public HunterConnectionManager(IOptions<HunterServersOptions> servers)
    {
        _servers = servers;
    }

    public async Task<string> PingAsync(string hunterId, string msg)
    {
        try
        {
            var conn = await GetOrCreateAsync(hunterId);
            return await conn.Bridge.InvokeAsync<string>("Ping", msg);
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return "";
        }
    }

    private Task<HunterConn> GetOrCreateAsync(string hunterId)
    {
        var lazy = _conns.GetOrAdd(hunterId, _ => new Lazy<Task<HunterConn>>(() => CreateAsync(hunterId)));
        return lazy.Value;
    }

    private async Task<HunterConn> CreateAsync(string hunterId)
    {
        var target = _servers.Value.HunterServers.FirstOrDefault(t => t.Id == hunterId)
                     ?? throw new InvalidOperationException($"Unknown Hunter server id: {hunterId}");

        if (string.IsNullOrWhiteSpace(target.ServiceUser) || string.IsNullOrWhiteSpace(target.ServicePassword))
            throw new InvalidOperationException($"Hunter server '{hunterId}' is missing ServiceUser/ServicePassword in config.");

        // 1) Login to Hunter AuthHub and get JWT
        var authHub = new HubConnectionBuilder()
            .WithUrl($"{target.BaseUrl.TrimEnd('/')}/hubs/auth")
            .WithAutomaticReconnect()
            .Build();

        await authHub.StartAsync();

        var loginReq = new HunterLoginRequest { User = target.ServiceUser, Password = target.ServicePassword };
        var loginRes = await authHub.InvokeAsync<HunterLoginResult>("Login", loginReq);

        if (loginRes is null || string.IsNullOrWhiteSpace(loginRes.Jwt))
            throw new InvalidOperationException("Hunter login failed: no JWT returned.");

        await authHub.StopAsync();
        await authHub.DisposeAsync();

        // 2) Connect to Hunter observer bridge with the JWT
        var bridge = new HubConnectionBuilder()
            .WithUrl($"{target.BaseUrl.TrimEnd('/')}/hubs/observer-bridge", o =>
            {
                o.AccessTokenProvider = () => Task.FromResult(loginRes.Jwt)!;
            })
            .WithAutomaticReconnect()
            .Build();

        await bridge.StartAsync();

        return new HunterConn(hunterId, bridge);
    }

    private sealed record HunterConn(string HunterId, HubConnection Bridge);

    private sealed class HunterLoginRequest
    {
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
    }

    private sealed class HunterLoginResult
    {
        public string Jwt { get; set; } = "";
        public int ServerId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int Version { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
