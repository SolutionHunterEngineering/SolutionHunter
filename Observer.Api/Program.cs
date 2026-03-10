// Observer.Api/Program.cs

using Microsoft.AspNetCore.Authentication.Cookies;
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
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// AuthService lives in Observer.Api.Services (per your file)
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
// Hunter server list (Option 2)
// -------------------------
builder.Services.Configure<HunterServersOptions>(
    builder.Configuration.GetSection("HunterServers"));

// Services used by ObserverHub
builder.Services.AddSingleton<HunterHealthService>();
builder.Services.AddSingleton<HunterConnectionManager>();
builder.Services.AddSingleton<HunterConnectionManager>(); // idempotent even if duplicated; keep only one in your file

var app = builder.Build();

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

app.Run();
