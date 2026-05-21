using BirthdayReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BirthdayReminder.Infrastructure.Persistences;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Reminder> Reminders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.BirthdayNotificationUtc);
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new
            {
                x.UserId,
                x.Type,
                x.ScheduledDate
            }).IsUnique();
        });
    }
}
