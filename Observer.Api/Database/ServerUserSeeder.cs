using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Observer.Api.Workspaces;
using Observer.Shared.Identity;

namespace Observer.API.Database;

public static class ServerUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Make sure DB is there
        await db.Database.MigrateAsync();

        // If there are no servers, nothing to do
        var servers = await db.Servers.ToListAsync();
        if (servers.Count == 0)
            return;

        // Pick the first server as the default for seeding
        var defaultServer = servers[0];
        // Use a string key for HunterServerId (for now, the DB ServerId as string)
        // Later you can swap this to the real GUID from the HunterServers config.
        var defaultHunterKey = defaultServer.ServerId.ToString();

        var users = await db.Set<AppUser>().ToListAsync();

        foreach (var user in users)
        {
            var hasRow = await db.ServerUsers.AnyAsync(su =>
                su.EngineerUserId == user.Id &&
                su.HunterServerId == defaultHunterKey);

            if (!hasRow)
            {
                db.ServerUsers.Add(new ServerUser
                {
                    Id = Guid.NewGuid(),
                    EngineerUserId = user.Id,
                    HunterServerId = defaultHunterKey,
                    CreatedUtc = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
