using Microsoft.EntityFrameworkCore;
using VirtualRouletteApi.Domain;

namespace VirtualRouletteApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Bet> Bets => Set<Bet>();
    public DbSet<Jackpot> Jackpots => Set<Jackpot>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .HasMaxLength(64);
        
        modelBuilder.Entity<Jackpot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount);
            e.Property(x => x.UpdatedAt);
        });
    }
}
