using API.Database;
using Hunter.Api.Configuration;
using Hunter.Api.Hubs;
using Hunter.Api.Services;
using Hunter.Api.Services.Auth;
using Hunter.Auth;
using IdentityDomain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

//
// ------------------------------------------------------------
// 1) Detect Role (QueenBee / WorkerBee)
// ------------------------------------------------------------
// Priority:
//   1) Command line: --role QueenBee
//   2) Environment variable: HUNTER_ROLE
//   3) Default: QueenBee
//

string? roleFromArgs = null;

for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i].Equals("--role", StringComparison.OrdinalIgnoreCase))
    {
        roleFromArgs = args[i + 1];
        break;
    }
}

string role =
    roleFromArgs ??
    Environment.GetEnvironmentVariable("HUNTER_ROLE") ??
    "QueenBee";

//
// ------------------------------------------------------------
// 2) Load role-specific configuration
// ------------------------------------------------------------
// Base config already loaded by WebApplication.CreateBuilder()
// Now we add role override.
//

builder.Configuration.AddJsonFile(
    $"appsettings.{role}.json",
    optional: true,
    reloadOnChange: true);

//
// ------------------------------------------------------------
// 3) Standard ASP.NET / Hunter setup
// ------------------------------------------------------------

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

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

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

builder.Services.AddAuthentication();
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
// This will create HunterDb if it doesnâ€™t exist and then seed Admin + Chuck.
await AuthSeed.RunAsync(app.Services, seedPath);

//
// ------------------------------------------------------------
// 7) Log startup identity (VERY useful)
// ------------------------------------------------------------

var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("HunterStartup");

logger.LogInformation("====================================");
logger.LogInformation("Hunter.Api Starting");
logger.LogInformation("Role: {Role}", role);
logger.LogInformation("Environment: {Env}", app.Environment.EnvironmentName);
logger.LogInformation("====================================");

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
app.MapHub<AuthHub>("hubs/auth");
app.MapFallbackToFile("index.html");

app.Run();
