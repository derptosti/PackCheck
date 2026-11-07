using Microsoft.EntityFrameworkCore;
using PackCheck.Api.Entities;

namespace PackCheck.Api.Data;

public class PackCheckDbContext(DbContextOptions<PackCheckDbContext> options) : DbContext(options)
{
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<PackingItem> PackingItems => Set<PackingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.Property(trip => trip.Name).HasMaxLength(100).IsRequired();
            entity.Property(trip => trip.TripType).HasMaxLength(40).IsRequired();
            entity.HasMany(trip => trip.Items).WithOne(item => item.Trip)
                .HasForeignKey(item => item.TripId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PackingItem>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.Property(item => item.CategoryKey).HasMaxLength(40);
            entity.ToTable(table => table.HasCheckConstraint("CK_PackingItem_Quantities",
                "[RequiredQuantity] BETWEEN 1 AND 999 AND [PackedQuantity] BETWEEN 0 AND [RequiredQuantity]"));
        });
    }
}
