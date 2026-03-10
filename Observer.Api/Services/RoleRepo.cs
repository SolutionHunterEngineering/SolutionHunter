// Observer.Api/Services/Auth/RoleRepo.cs
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Observer.API.Database;
using Observer.Shared.Identity;

namespace Observer.Auth
{
    public interface IRoleRepo
    {
        Task<string[]> RolesForUserAsync(int serverId, int userId);
        Task<bool> IsUserInRoleAsync(int serverId, int userId, string roleName);
    }

    /// <summary>
    /// Simple role repository over Identity tables.
    /// NOTE: serverId is currently ignored because roles are global;
    /// we keep the parameter so the signature stays stable if we
    /// later add per-server scopes.
    /// </summary>
    public sealed class RoleRepo : IRoleRepo
    {
        private readonly DataContext _db;

        public RoleRepo(DataContext db)
        {
            _db = db;
        }

        public async Task<string[]> RolesForUserAsync(int serverId, int userId)
        {
            // serverId intentionally not used for now
            return await (
                from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                where ur.UserId == userId
                select r.Name!
            ).ToArrayAsync();
        }

        public async Task<bool> IsUserInRoleAsync(int serverId, int userId, string roleName)
        {
            var norm = roleName.ToUpperInvariant();

            // serverId intentionally not used for now
            var exists = await (
                from ur in _db.UserRoles
                join r in _db.Roles on ur.RoleId equals r.Id
                where ur.UserId == userId && r.NormalizedName == norm
                select 1
            ).AnyAsync();

            return exists;
        }
    }
}
