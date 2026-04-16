using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Observer.Shared.Identity;
using Observer.Api.Workspaces;
using Observer.Api.Services.Hunters;

namespace Observer.API.Database;

public class DataContext : IdentityDbContext<
    AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<AppRole> Roles { get; set; } = default!;
    public DbSet<AppUserRole> UserRoles { get; set; } = default!;
public DbSet<ServerUser> ServerUsers { get; set; } = default!;
    public DbSet<ProblemCase> ProblemCases { get; set; } = default!;
    public DbSet<HunterServerRecord> HunterServers { get; set; } = default!;

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

        builder.Entity<HunterServerRecord>(e =>
        {
            e.ToTable("HunterServers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).IsRequired().HasMaxLength(500);
            e.Property(x => x.BaseUrl).IsRequired().HasMaxLength(256);
            e.Property(x => x.Kind).IsRequired().HasMaxLength(32);
            e.Property(x => x.IsRunning).IsRequired();
            e.Property(x => x.WorkCapacity).IsRequired();
            e.Property(x => x.CurrentWorkLoad).IsRequired();
            e.Property(x => x.LastCheckedUtc).IsRequired();
            e.Property(x => x.LatencyMs);
            e.Property(x => x.Error).HasMaxLength(500);
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
