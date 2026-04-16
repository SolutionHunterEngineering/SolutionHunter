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

        // If there are no hunter servers, nothing to do
        var servers = await db.HunterServers.ToListAsync();
        if (servers.Count == 0)
            return;

        var defaultHunterKey = servers[0].Id;

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
