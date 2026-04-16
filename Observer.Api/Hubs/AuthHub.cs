using Microsoft.AspNetCore.SignalR;
using Observer.Shared.DTOs.Auth;

// IMPORTANT:
// These interfaces should come from your shared Auth project/namespace.
// If Rider can’t resolve them, tell me what namespace they live in and I’ll adjust.
using Observer.Auth;

namespace Observer.Api.Hubs;

public sealed record RoleRefreshResult(int Version, string[] Roles);

public sealed class AuthHub : Hub
{
    private readonly Observer.Auth.IRoleRepo _roles;
    private readonly Observer.Auth.ILoginIssuer _loginIssuer;

    public AuthHub(Observer.Auth.IRoleRepo roles, Observer.Auth.ILoginIssuer loginIssuer)
    {
        _roles = roles;
        _loginIssuer = loginIssuer;
    }

    /// <summary>
    /// Login used by Observer.Client NavBar.
    /// MUST return AuthResponse (Success/Error/Jwt/UserName/UserId).
    /// </summary>
    public async Task<AuthResponse> Login(LoginRequest request)
    {
        if (request is null)
            return AuthResponse.Fail("Login request was null.");

        try
        {
            // Current behavior: fixed serverId until you add server selection at login time.
            // If LoginRequest later gains ServerId, we’ll pick it up without breaking older clients.
            var serverId = 1;

            // Try to read ServerId dynamically (optional / future-proof)
            try
            {
                var prop = request.GetType().GetProperty("ServerId");
                if (prop?.PropertyType == typeof(int))
                {
                    var value = (int?)prop.GetValue(request);
                    if (value.HasValue && value.Value > 0)
                        serverId = value.Value;
                }
            }
            catch
            {
                // ignore; fallback stays serverId = 1
            }

            // Issue token (your existing code path)
            var lr = await _loginIssuer.LoginIssueTokenAsync(serverId, request.User, request.Password);

            // Enforce invariant:
            // Success=false => Jwt MUST be empty and Error MUST be meaningful
            if (lr is null || string.IsNullOrWhiteSpace(lr.Jwt))
                return AuthResponse.Fail("Invalid username or password.");

            // Canonical response consumed by the client NavBar
            return AuthResponse.Ok(
                userId: lr.UserId,
                userName: lr.UserName ?? request.User,
                jwt: lr.Jwt
            );
        }
        catch (Exception ex)
        {
            // Don’t throw to the client for normal auth failures; return a deterministic failure response.
            // Logging here keeps server-side visibility.
            Console.WriteLine($"[AuthHub.Login] EX: {ex}");
            return AuthResponse.Fail("Login failed due to a server error.");
        }
    }

    public async Task<RoleRefreshResult> GetRoles(int serverId, int userId, int tokenVersion)
    {
        var roles = await _roles.RolesForUserAsync(serverId, userId);

        // Placeholder: replace when you track per-user role/token versioning
        var currentVersion = tokenVersion;

        return new RoleRefreshResult(currentVersion, roles);
    }
}
