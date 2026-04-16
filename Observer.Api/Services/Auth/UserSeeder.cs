using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Observer.Shared.Identity;
using System.Text.Json;

namespace Observer.API.Database;

public static class UserSeeder
{
    private const string SeedFileName = "UserSeedData.json";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        await db.Database.MigrateAsync();

        // ------------------------------------------------------------
        // Load JSON seed file
        // ------------------------------------------------------------
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        // JSON is currently at: Observer.Api/Services/Auth/UserSeedData.json
        var filePath = Path.Combine(env.ContentRootPath, "Services", "Auth", SeedFileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"User seed data file not found at: {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var root = JsonSerializer.Deserialize<UserSeedRoot>(json)
                   ?? throw new Exception("Invalid JSON seed file format.");

        // JSON: Servers â†’ Users â†’ Roles. We still iterate Servers
        // even though Observer logically has only one.
        foreach (var seedServer in root.Servers)
        {
            // --------------------------------------------------------
            // 1) Ensure roles exist
            // --------------------------------------------------------
            foreach (var roleName in seedServer.Roles)
            {
                var normalized = roleName.ToUpperInvariant();

                var role = await roleManager.Roles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.NormalizedName == normalized);

                if (role == null)
                {
                    await roleManager.CreateAsync(new AppRole
                    {
                        Name = roleName,
                        NormalizedName = normalized
                    });
                }
            }

            // --------------------------------------------------------
            // 2) Process users in this server block
            // --------------------------------------------------------
            foreach (var u in seedServer.Users)
            {
                var existingUser = await userManager.FindByNameAsync(u.UserName);

                if (existingUser == null)
                {
                    // ------------------------------------------------
                    // USER DOES NOT EXIST â†’ CREATE NEW
                    // ------------------------------------------------
                    var newUser = new AppUser
                    {
                        UserName    = u.UserName,
                        Email       = u.Email,
                        PhoneNumber = u.PhoneNumber,

                        FirstName    = u.FirstName,
                        LastName     = u.LastName,

                        // Optional extra fields you defined:
                        KnownAs      = u.KnownAs,
                        Organization = u.Organization,
                        AltPhone     = u.AltPhone,
                        City         = u.City,
                        State        = u.State,
                        Country      = u.Country,
                        ZipCode      = u.ZipCode,
                        Question     = u.Question,
                        Answer       = u.Answer,

                        EmailConfirmed       = true,
                        PhoneNumberConfirmed = true,
                        UserType             = 0
                    };

                    var createResult = await userManager.CreateAsync(newUser, u.SeedPassword);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed creating user {u.UserName}: {errors}");
                    }

                    existingUser = newUser;
                }
                else
                {
                    // ------------------------------------------------
                    // USER EXISTS â†’ UPDATE PROFILE FIELDS FROM JSON
                    // (only fields you actually want driven by JSON)
                    // ------------------------------------------------
                    bool changed = false;

                    if (existingUser.FirstName != u.FirstName)
                    {
                        existingUser.FirstName = u.FirstName;
                        changed = true;
                    }

                    if (existingUser.LastName != u.LastName)
                    {
                        existingUser.LastName = u.LastName;
                        changed = true;
                    }

                    if (existingUser.Email != u.Email)
                    {
                        existingUser.Email = u.Email;
                        changed = true;
                    }

                    if (existingUser.PhoneNumber != u.PhoneNumber)
                    {
                        existingUser.PhoneNumber = u.PhoneNumber;
                        changed = true;
                    }

                    // Optional: keep these only if you truly want JSON to own them
                    if (existingUser.City != u.City)
                    {
                        existingUser.City = u.City;
                        changed = true;
                    }

                    if (existingUser.State != u.State)
                    {
                        existingUser.State = u.State;
                        changed = true;
                    }

                    if (changed)
                    {
                        var updateResult = await userManager.UpdateAsync(existingUser);
                        if (!updateResult.Succeeded)
                        {
                            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                            throw new Exception($"Failed updating user {u.UserName}: {errors}");
                        }
                    }
                }

                // ----------------------------------------------------
                // 3) Ensure roles for this user
                // ----------------------------------------------------
                foreach (var roleName in u.Roles)
                {
                    if (!await userManager.IsInRoleAsync(existingUser, roleName))
                    {
                        await userManager.AddToRoleAsync(existingUser, roleName);
                    }
                }
            }
        }
    }
}

// =====================================================================
// JSON MODELS
// =====================================================================
public sealed class UserSeedRoot
{
    public List<UserSeedServer> Servers { get; set; } = new();
}

public sealed class UserSeedServer
{
    public string Name { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<UserSeedRecord> Users { get; set; } = new();
}

public sealed class UserSeedRecord
{
    public string UserName { get; set; } = string.Empty;
    public string SeedPassword { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string KnownAs { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string AltPhone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}
