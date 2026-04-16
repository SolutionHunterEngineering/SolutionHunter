using API.Database;
using Hunter.Api.Configuration;
using Hunter.Api.Hubs;
using Hunter.Api.Services;
using Hunter.Api.Services.Auth;
using Hunter.Auth;
using IdentityDomain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.Configure<HunterSettings>(
    builder.Configuration.GetSection("Hunter"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

//
// ------------------------------------------------------------
// 4) Database
// ------------------------------------------------------------
// NOTE:
// This assumes your connection string key is "DefaultConnection".
// If your appsettings uses a different key, change it here.
//

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));

//
// ------------------------------------------------------------
// 5) Identity
// ------------------------------------------------------------
// Your DataContext is IdentityDbContext<AppUser, AppRole, int, ...>
// so we must register Identity with AppUser/AppRole, not plain IdentityRole.
//

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddDefaultTokenProviders();

// Load the same signing cert used by JwtTokenService so [Authorize] can validate tokens.
var jwtCfg = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var rawThumb = Environment.GetEnvironmentVariable("HUNTER_JWT_SIGNING_THUMBPRINT")
               ?? jwtCfg.SigningCertThumbprint;
var thumb = rawThumb.Replace(" ", "").Trim().ToUpperInvariant();

static X509Certificate2? FindCert(StoreLocation loc, string t)
{
    using var store = new X509Store(StoreName.My, loc);
    store.Open(OpenFlags.ReadOnly);
    var matches = store.Certificates.Find(X509FindType.FindByThumbprint, t, validOnly: false);
    return matches.Count > 0 ? matches[0] : null;
}

var signingCert = FindCert(StoreLocation.CurrentUser, thumb)
               ?? FindCert(StoreLocation.LocalMachine, thumb)
               ?? throw new InvalidOperationException(
                   $"JWT signing cert not found (thumbprint '{thumb}'). " +
                   "Import the PFX into CurrentUser\\My or LocalMachine\\My.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer            = jwtCfg.Issuer,
            ValidAudience          = jwtCfg.Audience,
            IssuerSigningKey       = new X509SecurityKey(signingCert),
            ValidateIssuerSigningKey = true,
            ValidateLifetime       = true,
        };
        // SignalR passes the token as ?access_token= on the WebSocket upgrade request
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) &&
                    ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

//
// ------------------------------------------------------------
// 6) Hunter auth services
// ------------------------------------------------------------

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserAuthService>();
builder.Services.AddScoped<ILoginIssuer, LoginIssuer>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRoleRepo, RoleRepo>();

// JWT token generator
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// --------- SEED USERS / ROLES ON STARTUP ----------
var seedPath = Path.Combine(AppContext.BaseDirectory, "Services", "Auth", "UserSeedData.json");
// This will create HunterDb if it doesnt exist and then seed Admin + Chuck.
await AuthSeed.RunAsync(app.Services, seedPath);

//
// ------------------------------------------------------------
// 7) Log startup identity (VERY useful)
// ------------------------------------------------------------

var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("HunterStartup");

logger.LogInformation("Hunter.Api Starting");

//
// ------------------------------------------------------------
// 8) Pipeline
// ------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<HunterHub>("/hubs/hunter");
app.MapHub<AuthHub>("/hubs/auth");
app.MapHub<ObserverBridgeHub>("/hubs/observer-bridge");
app.MapFallbackToFile("index.html");

app.Run();
