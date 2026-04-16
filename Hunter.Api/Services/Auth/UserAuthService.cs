using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Hunter.Auth;

namespace Hunter.Api.Services.Auth
{
    public class UserAuthService
    {
        private readonly ILoginIssuer _issuer;
        private readonly IHttpContextAccessor _http;

        public UserAuthService(ILoginIssuer issuer, IHttpContextAccessor http)
        {
            _issuer = issuer;
            _http = http;
        }

        public async Task<bool> LoginAsync(int serverId, string userName, string password)
        {
            var res = await _issuer.LoginIssueTokenAsync(serverId, userName, password);
            if (res == null) return false;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, res.UserId.ToString()),
                new Claim(ClaimTypes.Name, res.UserName),
                new Claim("sid", res.ServerId.ToString())
            };
            foreach (var r in res.Roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(id);
            await _http.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return true;
        }

        public async Task LogoutAsync()
        {
            if (_http.HttpContext != null)
                await _http.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
