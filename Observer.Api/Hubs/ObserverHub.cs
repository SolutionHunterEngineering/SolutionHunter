// Observer.Api/Hubs/ObserverHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Observer.Api.Options;
using Observer.Api.Services;
using Observer.Api.Services.Auth;
using Observer.Api.Services.Hunters;
using Observer.Shared.DTOs.Auth;
using Observer.Shared.DTOs.Hunters;

namespace Observer.Api.Hubs;

public sealed class ObserverHub : Hub
{
    private readonly IAuthService _auth;
    private readonly UserAuthService _userAuth;
    private readonly HunterConnectionManager _hunters;
    private readonly IOptions<HunterServersOptions> _hunterServers;
    private readonly HunterHealthService _hunterHealth;

    public ObserverHub(
        IAuthService auth,
        UserAuthService userAuth,
        HunterConnectionManager hunters,
        IOptions<HunterServersOptions> hunterServers,
        HunterHealthService hunterHealth)
    {
        _auth = auth;
        _userAuth = userAuth;
        _hunters = hunters;
        _hunterServers = hunterServers;
        _hunterHealth = hunterHealth;
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[ObserverHub] Connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public async Task<AuthResponse> Login(LoginRequest dto)
    {
        var result = await _auth.LoginAsync(dto);
        if (!result.Success) return result;

        var serverId = 1;
        var ok = await _userAuth.LoginAsync(serverId, dto.User, dto.Password);

        if (ok)
        {
            Context.Items["UserId"] = result.UserId;
            Context.Items["UserName"] = result.UserName;
            Context.Items["ServerId"] = serverId;
        }

        return result;
    }

    // ------------------------------------------------------------
    // Health (SQL-backed / TTL-gated logic is inside HunterHealthService)
    // ------------------------------------------------------------
    public Task<List<HunterHealthDto>> GetHunterHealth(TimeSpan? ttl = null)
    {
        var effectiveTtl = ttl ?? TimeSpan.FromSeconds(30);
        return _hunterHealth.GetStatusesAsync(effectiveTtl);
    }

    public async Task Logout()
    {
        await _userAuth.LogoutAsync();
        Context.Items.Remove("UserId");
        Context.Items.Remove("UserName");
        Context.Items.Remove("ServerId");
        Context.Items.Remove("SelectedHunterServerId");
    }

    // API-owned list for the dropdown
    public Task<List<HunterServerRow>> GetHunterServers()
    {
        var list = _hunterServers.Value.HunterServers
            .Select(s => new HunterServerRow
            {
                ServerId = s.Id,
                DisplayName = s.Name,
                BaseUrl = s.BaseUrl
            })
            .ToList();

        return Task.FromResult(list);
    }

    public Task SelectHunterServer(string hunterServerId)
    {
        Context.Items["SelectedHunterServerId"] = hunterServerId;
        return Task.CompletedTask;
    }

    public async Task<string> PingSelectedHunter(string msg)
    {
        if (!Context.Items.TryGetValue("SelectedHunterServerId", out var idObj) ||
            idObj is not string hunterId ||
            string.IsNullOrWhiteSpace(hunterId))
        {
            return "No Hunter selected.";
        }

        return await _hunters.PingAsync(hunterId, msg);
    }

    public async Task<bool> SelectHunterAndHandshake(string hunterServerId)
    {
        Context.Items["SelectedHunterServerId"] = hunterServerId;

        try
        {
            var reply = await _hunters.PingAsync(hunterServerId, "Observer handshake ping.");
            return !string.IsNullOrWhiteSpace(reply);
        }
        catch
        {
            return false;
        }
    }

    public Task<string> Echo(string msg) => Task.FromResult($"Observer.Echo::{msg}");

    public sealed class HunterServerRow
    {
        public string ServerId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string BaseUrl { get; set; } = "";
    }
}
