using Microsoft.EntityFrameworkCore;
using HabitTracker.Models;

namespace HabitTracker.Data;

public class HabitContext : DbContext
{
    public DbSet<Habit> Habits { get; set; }
    public DbSet<HabitLog> HabitLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HabitTracker"
        );
        Directory.CreateDirectory(dbFolder);

        var dbPath = Path.Combine(dbFolder, "habits.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Emoji).HasMaxLength(10);
            entity.Property(e => e.Color).HasMaxLength(20);
        });

        modelBuilder.Entity<HabitLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Habit)
                  .WithMany(h => h.Logs)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
