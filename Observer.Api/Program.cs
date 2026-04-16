// Observer.Api/Program.cs

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Observer.API.Database;
using Observer.Api.Hubs;
using Observer.Api.Options;
using Observer.Api.Services;
using Observer.Api.Services.Auth;
using Observer.Api.Services.Hunters;
using Observer.Auth;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR(o => o.EnableDetailedErrors = true);

// -------------------------
// EF Core + Identity
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("DefaultConnection is not configured.");

// AddDbContextFactory registers both IDbContextFactory<DataContext> (singleton)
// and DataContext itself (scoped) — satisfies Identity and HunterHealthService singleton.
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseSqlServer(connectionString));

// IMPORTANT:
// Your DataContext is IdentityDbContext<AppUser, AppRole, int, ...>
// So Identity is keyed by int. Keep everything aligned to that.
builder.Services
    .AddIdentity<Observer.Shared.Identity.AppUser, Observer.Shared.Identity.AppRole>(options =>
    {
        // dev-friendly; tighten later
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// -------------------------
// Auth / JWT issuing
// -------------------------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Canonical services live in Observer.Auth
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRoleRepo, RoleRepo>();
builder.Services.AddScoped<ILoginIssuer, LoginIssuer>();

// AuthService lives in Observer.Api.Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Used by ObserverHub.Login
builder.Services.AddScoped<UserAuthService>();

// -------------------------
// CORS (SignalR needs creds)
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7090",
                "http://localhost:5090"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// -------------------------
// Hunter server list
// -------------------------
builder.Services.Configure<HunterServersOptions>(
    builder.Configuration.GetSection("HunterServers"));

// Services used by ObserverHub
builder.Services.AddSingleton<HunterHealthService>();
builder.Services.AddSingleton<HunterConnectionManager>();

var app = builder.Build();

// ===== Create / migrate / seed ObserverDb on startup =====
await UserSeeder.SeedAsync(app.Services);

// ===== Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowClient");

app.UseAuthentication();
app.UseAuthorization();

// ===== SignalR =====
app.MapHub<AuthHub>("/hubs/auth");
app.MapHub<ObserverHub>("/hubs/observer");

app.MapControllers();

app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.Run();