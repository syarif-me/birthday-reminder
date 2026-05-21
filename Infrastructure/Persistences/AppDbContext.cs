using BirthdayReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BirthdayReminder.Infrastructure.Persistences;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.BirthdayNotificationUtc);
        });
    }
}
