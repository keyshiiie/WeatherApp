using Microsoft.EntityFrameworkCore;
using WeatherApp.Core.Entities;

namespace WeatherApp.Core.Data;

public class AppDbContext : DbContext
{
    private readonly string? _connectionString;

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<CityEntity> Cities { get; set; }
    public DbSet<WeatherCacheEntity> WeatherCache { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString))
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CityEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.AddedAt).IsRequired();
            entity.Property(e => e.IsLastSelected).IsRequired();
            entity.Property(e => e.IsFavorite).IsRequired();
            entity.Property(e => e.IsRecent).IsRequired();
            entity.Property(e => e.LastSearchedAt).IsRequired();

            entity.HasIndex(e => e.Name).HasDatabaseName("IX_Cities_Name");

            entity.HasIndex(e => e.IsLastSelected).HasDatabaseName("IX_Cities_IsLastSelected");

            entity.HasIndex(e => new { e.Name, e.Country })
                  .IsUnique()
                  .HasDatabaseName("IX_Cities_Name_Country");

            entity.HasOne(e => e.WeatherCache)
                  .WithOne(w => w.City)
                  .HasForeignKey<WeatherCacheEntity>(w => w.CityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeatherCacheEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CityId).IsRequired();
            entity.Property(e => e.CityName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.WeatherDataJson).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.CachedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();

            entity.HasIndex(e => e.CityId).HasDatabaseName("IX_WeatherCache_CityId");

            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_WeatherCache_ExpiresAt");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<CityEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.AddedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<WeatherCacheEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CachedAt = DateTime.UtcNow;
                if (entry.Entity.ExpiresAt == default)
                {
                    entry.Entity.ExpiresAt = DateTime.UtcNow.AddMinutes(30);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}