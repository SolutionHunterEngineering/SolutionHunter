using Microsoft.AspNetCore.SignalR;
using Hunter.Auth;
using Hunter.Shared.DTOs.Auth;

namespace Hunter.Auth;

public sealed record RoleRefreshResult(int Version, string[] Roles);

public sealed class AuthHub : Hub
{
    private readonly IRoleRepo _roles;
    private readonly ILoginIssuer _loginIssuer;

    public AuthHub(IRoleRepo roles, ILoginIssuer loginIssuer)
    {
        _roles = roles;
        _loginIssuer = loginIssuer;
    }

    /// <summary>
    /// JWT-style login: takes a LoginRequest, returns a LoginResult with JWT + roles.
    /// </summary>
    public async Task<LoginResult> Login(LoginRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        // TODO: when you have multiple servers, pass the actual selected serverId
        var serverId = 1;

        // This calls into the internal token issuer (TokenLoginResult)
        var internalResult = await _loginIssuer.LoginIssueTokenAsync(
            serverId,
            request.User,
            request.Password
        );

        // Map internal TokenLoginResult -> Shared LoginResult DTO
        return new LoginResult
        {
            Jwt = internalResult.Jwt,
            ServerId = internalResult.ServerId,
            UserId = internalResult.UserId,
            UserName = internalResult.UserName,
            Version = internalResult.Version,
            Roles = internalResult.Roles
        };
    }

    /// <summary>
    /// Existing role refresh endpoint, kept as-is for now.
    /// </summary>
    public async Task<RoleRefreshResult> GetRoles(int serverId, int userId, int tokenVersion)
    {
        // If you later store per-user version, compare tokenVersion and bump.
        var roles = await _roles.RolesForUserAsync(serverId, userId);
        var currentVersion = tokenVersion; // placeholder; replace with real version when you add it
        return new RoleRefreshResult(currentVersion, roles);
    }
}
