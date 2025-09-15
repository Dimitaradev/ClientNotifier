using ClientNotifier.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientNotifier.Data;

public class NotifierContext : DbContext
{
    public NotifierContext(DbContextOptions<NotifierContext> options) : base(options) { }

    public DbSet<People> People { get; set; }
    public DbSet<NamedayMapping> NamedayMappings { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // People entity configuration
        modelBuilder.Entity<People>(entity =>
        {
            entity.HasIndex(p => p.EGN).IsUnique();
            entity.HasIndex(p => p.Birthday);
            entity.HasIndex(p => p.Nameday);
            entity.HasIndex(p => new { p.FirstName, p.LastName });
            
            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("datetime('now')");
        });

        // NamedayMapping entity configuration
        modelBuilder.Entity<NamedayMapping>(entity =>
        {
            entity.HasIndex(n => n.Name);
            entity.HasIndex(n => new { n.Month, n.Day });
            
            // Ensure month is between 1 and 12
            entity.Property(n => n.Month)
                .HasAnnotation("MinValue", 1)
                .HasAnnotation("MaxValue", 12);
                
            // Ensure day is between 1 and 31
            entity.Property(n => n.Day)
                .HasAnnotation("MinValue", 1)
                .HasAnnotation("MaxValue", 31);
                
            // Remove the DateTime Nameday property from database
            entity.Ignore(n => n.DateDisplay);
        });

        // NotificationLog configuration
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasIndex(n => new { n.PersonId, n.Type, n.Channel, n.SentAtUtc });
        });
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is People && e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            ((People)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
