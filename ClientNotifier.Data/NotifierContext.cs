using ClientNotifier.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientNotifier.Data;

public class NotifierContext : DbContext
{
    public NotifierContext(DbContextOptions<NotifierContext> options) : base(options) { }

    public DbSet<People> People { get; set; }
    public DbSet<NamedayMapping> Mapping { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NamedayMapping>()
            .HasIndex(n => n.Name)
            .IsUnique();
    }
}
