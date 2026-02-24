using Backend.Domain.Common;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using Backend.Infrastructure.Identity;

namespace Backend.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PortfolioSlug).IsRequired().HasMaxLength(64);
            e.HasIndex(u => u.PortfolioSlug).IsUnique();
        });

        builder.Entity<Project>(e =>
        {
            e.ToTable("projects");
            
            e.Property(p => p.Status).HasConversion<string>();
            
            e.HasIndex(p => new { p.OwnerId, p.DisplayOrder }).IsUnique();
            
            e.HasIndex(p => p.OwnerId)
                .IsUnique()
                .HasFilter("\"IsPinned\" = true");
                
            e.HasIndex(p => new { p.OwnerId, p.IsPinned, p.DisplayOrder })
                .IsDescending(false, true, false);
                
            e.HasOne<Asset>()
                .WithMany()
                .HasForeignKey(p => p.CoverAssetId)
                .OnDelete(DeleteBehavior.Restrict);
                
            e.HasOne<Asset>()
                .WithMany()
                .HasForeignKey(p => p.DemoVideoAssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Technology>(e =>
        {
            e.ToTable("technologies");
            
            e.HasIndex(t => t.Slug).IsUnique();
            e.HasIndex(t => t.Name).IsUnique();
        });

        builder.Entity<ProjectTechnology>(e =>
        {
            e.ToTable("project_technologies");
            
            e.HasKey(pt => new { pt.ProjectId, pt.TechnologyId });
            
            e.HasOne<Project>()
                .WithMany()
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            e.HasOne<Technology>()
                .WithMany()
                .HasForeignKey(pt => pt.TechnologyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Asset>(e =>
        {
            e.ToTable("assets");
            
            e.Property(a => a.Provider).HasConversion<string>();
            e.Property(a => a.ResourceType).HasConversion<string>();
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries();
        var utcNow = DateTime.UtcNow;
        
        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    if (baseEntity.CreatedAt == default)
                    {
                        baseEntity.CreatedAt = utcNow;
                    }
                    baseEntity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = utcNow;
                }
            }
            else if (entry.Entity is User userEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    if (userEntity.CreatedAt == default)
                    {
                        userEntity.CreatedAt = utcNow;
                    }
                    userEntity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    userEntity.UpdatedAt = utcNow;
                }
            }
        }
    }
}
