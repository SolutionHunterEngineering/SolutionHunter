// Observer.Api/Services/Auth/LoginIssuer.cs
using Observer.Api.Services;
using Observer.Shared.DTOs.Auth;

namespace Observer.Auth;

public interface ILoginIssuer
{
    Task<LoginResult> LoginIssueTokenAsync(int serverId, string userName, string password);
}

public sealed class LoginIssuer : ILoginIssuer
{
    private readonly IAuthService _auth;
    private readonly IJwtTokenService _jwt;
    private readonly IRoleRepo _roles;

    public LoginIssuer(IAuthService auth, IJwtTokenService jwt, IRoleRepo roles)
    {
        _auth = auth;
        _jwt = jwt;
        _roles = roles;
    }

    public async Task<LoginResult> LoginIssueTokenAsync(int serverId, string userName, string password)
    {
        var auth = await _auth.LoginAsync(new LoginRequest(userName, password));

        if (!auth.Success)
            throw new InvalidOperationException(auth.Error ?? "Login failed.");

        var ver = 1;
        var roles = await _roles.RolesForUserAsync(serverId, auth.UserId);

        var token = _jwt.CreateAccessToken(serverId, auth.UserId, auth.UserName!, ver, roles);

        return new LoginResult(
            token,
            serverId,
            auth.UserId,
            auth.UserName!,
            ver,
            roles
        );
    }
}
