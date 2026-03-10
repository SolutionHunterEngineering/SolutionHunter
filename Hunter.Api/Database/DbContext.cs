using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hunter.Auth;
using IdentityDomain;

namespace API.Database;

public class DataContext : IdentityDbContext<
    AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<AppRole> Roles { get; set; } = default!;
    public DbSet<AppUserRole> UserRoles { get; set; } = default!;
    public DbSet<Server> Servers { get; set; } = default!;

    ////        public DbSet<AppUser> Users { get; set; } Identity handles this guy

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Table names ï¿½ keep Users custom, others standard (or rename consistently)
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("AspNetRoles");
        builder.Entity<AppUserRole>().ToTable("AspNetUserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");

        builder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => new { u.ServerId, u.NormalizedUserName }).IsUnique();
            e.Property(u => u.ServerId).IsRequired();
        });

        builder.Entity<AppRole>(e =>
        {
            e.HasIndex(r => new { r.ServerId, r.NormalizedName }).IsUnique();
            e.Property(r => r.ServerId).IsRequired();
        });

        builder.Entity<AppUserRole>(e =>
        {
            e.HasKey(ur => new { ur.ServerId, ur.UserId, ur.RoleId }); // composite
            e.Property(ur => ur.ServerId).IsRequired();
        });

        // FK-ish relationships (no navs required)
        builder.Entity<Server>(e =>
        {
            e.ToTable("Servers");
            e.HasKey(s => s.ServerId);
            e.Property(s => s.Name).IsRequired().HasMaxLength(100);
            e.Property(s => s.Kind).HasMaxLength(32);
        });
    }
}
