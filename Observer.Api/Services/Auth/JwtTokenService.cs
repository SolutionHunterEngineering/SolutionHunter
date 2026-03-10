using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Observer.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Observer";
    public string Audience { get; set; } = "Observer";

    // NEW: identify the cert in Windows Cert Store (LocalMachine\My)
    public string SigningCertThumbprint { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;
}

public interface IJwtTokenService
{
    string CreateAccessToken(int serverId, int userId, string userName, int version, IEnumerable<string> roles, string? audienceOverride = null);
    X509Certificate2 GetSigningCertificate();
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly X509Certificate2 _signingCert;
    private readonly SigningCredentials _creds;

    public JwtTokenService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));

        if (string.IsNullOrWhiteSpace(_opt.SigningCertThumbprint))
            throw new InvalidOperationException("Jwt.SigningCertThumbprint is empty. Provide the LocalMachine\\My certificate thumbprint.");

        _signingCert = LoadCertFromLocalMachineMy(_opt.SigningCertThumbprint);

        if (!_signingCert.HasPrivateKey)
            throw new InvalidOperationException("JWT signing certificate was found but does not have a private key.");

        // Use the certificate private key for signing
        var signingKey = new X509SecurityKey(_signingCert);
        _creds = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);
    }

    public X509Certificate2 GetSigningCertificate() => _signingCert;

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

    private static X509Certificate2 LoadCertFromLocalMachineMy(string thumbprint)
    {
        // Thumbprints sometimes carry invisible chars; normalize hard.
        var normalized = new string(thumbprint.Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
        if (normalized.Length == 0)
            throw new InvalidOperationException("Jwt.SigningCertThumbprint contains no hex digits.");

        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        var matches = store.Certificates.Find(X509FindType.FindByThumbprint, normalized, validOnly: false);
        if (matches.Count == 0)
            throw new InvalidOperationException($"JWT signing certificate not found in LocalMachine\\My. Thumbprint: {normalized}");

        // Return a copy detached from the store
        return new X509Certificate2(matches[0]);
    }
}
