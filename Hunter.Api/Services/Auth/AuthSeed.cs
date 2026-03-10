using API.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityDomain;
using System.Text.Json;

namespace Hunter.Auth;

public static class AuthSeed
{
    private sealed class SeedRoot { public List<SeedServer>? Servers { get; set; } }
    private sealed class SeedServer
    {
        public string Name { get; set; } = "Hunter";
        public string? Kind { get; set; } = "Hunter";
        public string[]? Roles { get; set; }
        public List<SeedUser>? Users { get; set; }
    }
    private sealed class SeedUser
    {
        public string UserName { get; set; } = default!;
        public string? Email { get; set; }
        public string? SeedPassword { get; set; }
        public string[]? Roles { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? KnownAs { get; set; }
        public string? Organization { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AltPhone { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
    }

    public static async Task RunAsync(IServiceProvider sp, string jsonPath, string defaultPassword = "Test$12345")
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        await db.Database.MigrateAsync();

        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"[AuthSeed] Seed file not found: {jsonPath}");
            return;
        }

        var json = await File.ReadAllTextAsync(jsonPath);
        var model = JsonSerializer.Deserialize<SeedRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new SeedRoot();

        if (model.Servers == null || model.Servers.Count == 0) return;

        foreach (var s in model.Servers)
        {
            // Ensure server row
            var server = await db.Servers.FirstOrDefaultAsync(x => x.Name == s.Name);
            if (server == null)
            {
                server = new Server { Name = s.Name, Kind = s.Kind ?? s.Name };
                db.Servers.Add(server);
                await db.SaveChangesAsync();
            }
            var serverId = server.ServerId;

            // Roles
            if (s.Roles != null)
            {
                foreach (var roleName in s.Roles.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var norm = roleName.ToUpperInvariant();
                    var exists = await db.Roles.AsNoTracking()
                        .AnyAsync(r => r.ServerId == serverId && r.NormalizedName == norm);
                    if (!exists)
                    {
                        var create = await rm.CreateAsync(new AppRole
                        {
                            ServerId = serverId,
                            Name = roleName,
                            NormalizedName = norm
                        });
                        if (!create.Succeeded)
                            throw new InvalidOperationException("Create role failed: " +
                                string.Join("; ", create.Errors.Select(e => e.Description)));
                    }
                }
            }

            // Users + joins
            if (s.Users != null)
            {
                foreach (var u in s.Users)
                {
                    var norm = u.UserName.ToUpperInvariant();
                    var user = await db.Set<AppUser>().FirstOrDefaultAsync(x => x.ServerId == serverId && x.NormalizedUserName == norm);
                    if (user == null)
                    {
                        user = new AppUser
                        {
                            ServerId = serverId,
                            UserName = u.UserName,
                            NormalizedUserName = norm,
                            Email = u.Email,
                            EmailConfirmed = true,
                            FirstName = u.FirstName ?? "",
                            LastName = u.LastName ?? "",
                            KnownAs = u.KnownAs ?? "",
                            Organization = u.Organization ?? "",
                            PhoneNumber = u.PhoneNumber ?? "",
                            AltPhone = u.AltPhone ?? "",
                            City = u.City ?? "",
                            State = u.State ?? "",
                            Country = u.Country ?? "",
                            ZipCode = u.ZipCode ?? "",
                            Question = u.Question ?? "",
                            Answer = u.Answer ?? ""
                        };
                        var pwd = string.IsNullOrWhiteSpace(u.SeedPassword) ? defaultPassword : u.SeedPassword!;
                        var created = await um.CreateAsync(user, pwd);
                        if (!created.Succeeded)
                            throw new InvalidOperationException("Create user failed: " +
                                string.Join("; ", created.Errors.Select(e => e.Description)));
                    }

                    if (u.Roles != null)
                    {
                        foreach (var rn in u.Roles.Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            var normRole = rn.ToUpperInvariant();
                            var role = await db.Roles.AsNoTracking()
                                .FirstOrDefaultAsync(r => r.ServerId == serverId && r.NormalizedName == normRole);
                            if (role == null)
                                throw new InvalidOperationException($"Role not found for server {s.Name}: {rn}");

                            var exists = await db.UserRoles.AsNoTracking()
                                .AnyAsync(ur => ur.ServerId == serverId && ur.UserId == user.Id && ur.RoleId == role.Id);
                            if (!exists)
                            {
                                db.UserRoles.Add(new AppUserRole
                                {
                                    ServerId = serverId,
                                    UserId = user.Id,
                                    RoleId = role.Id
                                });
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
        }

        Console.WriteLine("[AuthSeed] Complete.");
    }
}
