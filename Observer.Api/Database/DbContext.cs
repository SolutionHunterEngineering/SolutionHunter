using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Observer.Shared.Identity;
using Observer.Auth;
using Observer.Api.Workspaces;

namespace Observer.API.Database;

public class DataContext : IdentityDbContext<
    AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<AppRole> Roles { get; set; } = default!;
    public DbSet<AppUserRole> UserRoles { get; set; } = default!;
    public DbSet<Server> Servers { get; set; } = default!;
    public DbSet<ServerUser> ServerUsers { get; set; } = default!;
    public DbSet<ProblemCase> ProblemCases { get; set; } = default!;

    ////        public DbSet<AppUser> Users { get; set; } Identity handles this guy

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("AspNetRoles");
        builder.Entity<AppUserRole>().ToTable("AspNetUserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");

        builder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.NormalizedUserName).IsUnique();
            // No ServerId here anymore
        });

        builder.Entity<AppRole>(e =>
        {
            e.HasIndex(r => r.NormalizedName).IsUnique();
            // No ServerId here either
        });

        // FK-ish relationships (no navs required)
        builder.Entity<Server>(e =>
        {
            e.ToTable("Servers");
            e.HasKey(s => s.ServerId);
            e.Property(s => s.Name).IsRequired().HasMaxLength(100);
            e.Property(s => s.Kind).HasMaxLength(32);
        });
        
        builder.Entity<ServerUser>(e =>
        {
            e.ToTable("ServerUsers");

            e.HasKey(x => x.Id);

            e.Property(x => x.EngineerUserId)
                .IsRequired();

            e.Property(x => x.HunterServerId)
                .IsRequired()
                .HasMaxLength(64);

            e.Property(x => x.CreatedUtc)
                .IsRequired();

            // One row per (Engineer, HunterServer)
            e.HasIndex(x => new { x.EngineerUserId, x.HunterServerId })
                .IsUnique();
        });

        builder.Entity<ProblemCase>(e =>
        {
            e.ToTable("ProblemCases");

            e.HasKey(x => x.Id);

            e.Property(x => x.ServerUserId)
                .IsRequired();

            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.CompanyName)
                .HasMaxLength(200);

            e.Property(x => x.ContactName)
                .HasMaxLength(200);

            e.Property(x => x.Description)
                .HasMaxLength(4000);

            e.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(32);

            e.Property(x => x.CreatedUtc)
                .IsRequired();

            // Optional: index by ServerUser to quickly list cases for a workspace
            e.HasIndex(x => x.ServerUserId);
        });
    }
}
