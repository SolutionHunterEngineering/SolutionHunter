using Hunter.Api.Services;
using Hunter.Auth;
using static Hunter.Auth.JwtTokenService;

namespace Hunter.Auth;

public interface ILoginIssuer
{
    Task<TokenLoginResult> LoginIssueTokenAsync(int serverId, string userName, string password);
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

    public async Task<TokenLoginResult> LoginIssueTokenAsync(int serverId, string userName, string password)
    {
        var auth = await _auth.LoginAsync(new LoginDto(userName, password));

        if (!auth.Success)
            throw new InvalidOperationException(auth.Error ?? "Login failed.");
       
        var ver = 1;
        var roles = await _roles.RolesForUserAsync(serverId, int.Parse(auth.UserId!));
        
        var token = _jwt.CreateAccessToken(serverId, int.Parse(auth.UserId!), auth.UserName!, ver, roles);

        return new TokenLoginResult(
            token,
            serverId,
            int.Parse(auth.UserId!),
            auth.UserName!,
            ver,
            roles
        );
    }
}
