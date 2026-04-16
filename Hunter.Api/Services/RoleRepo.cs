using API.Database;
using Microsoft.EntityFrameworkCore;
using IdentityDomain;

namespace Hunter.Auth;

public interface IRoleRepo
{
    Task<string[]> RolesForUserAsync(int serverId, int userId);
    Task<bool> IsUserInRoleAsync(int serverId, int userId, string roleName);
}

public sealed class RoleRepo : IRoleRepo
{
    private readonly DataContext _db;
    public RoleRepo(DataContext db) => _db = db;

    public async Task<string[]> RolesForUserAsync(int serverId, int userId)
    {
        return await (
            from ur in _db.UserRoles
            join r in _db.Roles
                on new { ur.RoleId, ur.ServerId } equals new { RoleId = r.Id, r.ServerId }
            where ur.ServerId == serverId && ur.UserId == userId
            select r.Name!
        ).ToArrayAsync();
    }


    public async Task<bool> IsUserInRoleAsync(int serverId, int userId, string roleName)
    {
        var norm = roleName.ToUpperInvariant();

        var exists = await (
            from ur in _db.UserRoles
            join r in _db.Roles
                on new { ur.RoleId, ur.ServerId }
                equals new { RoleId = r.Id, r.ServerId }
            where ur.ServerId == serverId
               && ur.UserId == userId
               && r.NormalizedName == norm
            select 1
        ).AnyAsync();

        return exists;
    }

}
