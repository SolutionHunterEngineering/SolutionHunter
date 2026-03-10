using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Hunter.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Hunter";
    public string Audience { get; set; } = "Hunter";

    /// <summary>
    /// Thumbprint of the X509 certificate (with private key) used to sign JWTs.
    /// This cert must be installed in Windows cert store:
    ///   - Dev (VS running as you): CurrentUser\My
    ///   - Service/IIS/Prod:         LocalMachine\My
    /// </summary>
    public string SigningCertThumbprint { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;

    /// <summary>
    /// Optional: public cert/thumbprint publishing later.
    /// Leave empty for now.
    /// </summary>
    public string PublicKeyPem { get; set; } = string.Empty;
}

public interface IJwtTokenService
{
    string CreateAccessToken(int serverId, int userId, string userName, int version, IEnumerable<string> roles, string? audienceOverride = null);
    string GetPublicKeyPem();
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly SigningCredentials _creds;

    public JwtTokenService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));

        var envThumb = Environment.GetEnvironmentVariable("HUNTER_JWT_SIGNING_THUMBPRINT");
        var thumb = NormalizeThumbprint(!string.IsNullOrWhiteSpace(envThumb)
            ? envThumb
            : _opt.SigningCertThumbprint);

        if (string.IsNullOrWhiteSpace(thumb))
            throw new InvalidOperationException(
                "JWT signing thumbprint is empty. Set machine env var HUNTER_JWT_SIGNING_THUMBPRINT " +
                "or configure Jwt:SigningCertThumbprint.");

        // Search order:
        // 1) CurrentUser\My (dev, running as your user)
        // 2) LocalMachine\My (service/IIS/prod)
        var cert =
            FindCertByThumbprint(StoreLocation.CurrentUser, thumb)
            ?? FindCertByThumbprint(StoreLocation.LocalMachine, thumb);

        if (cert is null)
        {
            throw new InvalidOperationException(
                $"JWT signing certificate not found by thumbprint '{thumb}'. Searched stores: CurrentUser\\My, LocalMachine\\My. " +
                "Import the PFX into the appropriate store and ensure it includes a private key.");
        }

        if (!cert.HasPrivateKey)
        {
            throw new InvalidOperationException(
                $"JWT signing certificate '{cert.Subject}' (thumbprint '{thumb}') was found but does NOT have a private key. " +
                "Re-import the PFX (must include private key) and ensure the running account has access to the private key.");
        }

        // Use the certificate's RSA key to sign JWTs (RS256)
        var key = new X509SecurityKey(cert);
        _creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
    }

    private static string NormalizeThumbprint(string? thumbprint)
        => (thumbprint ?? string.Empty).Replace(" ", string.Empty).Trim().ToUpperInvariant();

    private static X509Certificate2? FindCertByThumbprint(StoreLocation location, string normalizedThumbprint)
    {
        using var store = new X509Store(StoreName.My, location);
        try
        {
            store.Open(OpenFlags.ReadOnly);
            // FindByThumbprint expects no spaces; case-insensitive is fine, we normalize anyway.
            var matches = store.Certificates.Find(X509FindType.FindByThumbprint, normalizedThumbprint, validOnly: false);
            // Return the first match (thumbprints should be unique in a store)
            return matches.Count > 0 ? matches[0] : null;
        }
        finally
        {
            store.Close();
        }
    }

    public string CreateAccessToken(
        int serverId,
        int userId,
        string userName,
        int version,
        IEnumerable<string> roles,
        string? audienceOverride = null)
    {
        var now = DateTimeOffset.UtcNow;

        var claims = new List<Claim>
        {
            new("sid", serverId.ToString()),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new("ver", version.ToString())
        };
        claims.AddRange(roles.Select(r => new Claim("roles", r)));

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: audienceOverride ?? _opt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(_opt.AccessTokenMinutes).UtcDateTime,
            signingCredentials: _creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public string GetPublicKeyPem() => _opt.PublicKeyPem;

    public sealed record TokenLoginResult(string Jwt, int ServerId, int UserId, string UserName, int Version, string[] Roles);
}
