using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Hunter.Api.Services.Auth;   // UserAuthService
using Hunter.Api.Services;        // IAuthService, LoginDto, AuthResponse

namespace Hunter.Api.Hubs;

public sealed class HunterHub : Hub
{
    private readonly IAuthService _auth;         // existing service you already use for DTO-based login logic
    private readonly UserAuthService _userAuth;  // new cookie-signin wrapper

    public HunterHub(IAuthService auth, UserAuthService userAuth)
    {
        _auth = auth;
        _userAuth = userAuth;
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[HunterHub] Connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Validates credentials and issues cookie auth for the current connection's HttpContext.
    /// Returns the same AuthResponse DTO you already use so the client can display messages.
    /// </summary>
    public async Task<AuthResponse> Login(LoginDto dto)
    {
        // 1) Validate via your existing auth service
        var result = await _auth.LoginAsync(dto);
        if (!result.Success) return result;

        // 2) Issue cookie for this HttpContext via UserAuthService
        //    ServerId is carried in your system; for Hunter it’s 1 (or pass the real one the client selected)
        var serverId = 1; // adjust if the client selects a different server
        var ok = await _userAuth.LoginAsync(serverId, dto.UserName, dto.Password);
        if (ok)
        {
            // Keep per-connection state (optional but handy)
            Context.Items["UserId"] = result.UserId;
            Context.Items["UserName"] = result.UserName;
            Context.Items["ServerId"] = serverId;
        }

        return result;
    }

    /// <summary>
    /// Clears the auth cookie for the current HttpContext and drops any per-connection state.
    /// </summary>
    public async Task Logout()
    {
        await _userAuth.LogoutAsync();
        Context.Items.Remove("UserId");
        Context.Items.Remove("UserName");
        Context.Items.Remove("ServerId");
    }

    /// <summary>
    /// Returns a minimal “who am I” snapshot using the cookie principal after Login().
    /// </summary>
    public Task<object?> WhoAmI()
    {
        var http = Context.GetHttpContext();
        var user = http?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return Task.FromResult<object?>(null);

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var sid = user.FindFirst("sid")?.Value;
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();

        return Task.FromResult<object?>(new
        {
            name = user.Identity!.Name,
            id,
            sid,
            roles
        });
    }

    // quick smoke method
    public Task<string> Echo(string msg) => Task.FromResult($"server:{msg}");
}
