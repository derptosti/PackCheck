using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PackCheck.Core.Entities;

namespace PackCheck.Infrastructure.Data;

public class PackCheckDbContext : DbContext
{
    public PackCheckDbContext(
        DbContextOptions<PackCheckDbContext> options)
        : base(options)
    {
    }

    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<PackingItem> PackingItems => Set<PackingItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trip>()
            .ToTable("Trips");

        modelBuilder.Entity<PackingItem>()
            .ToTable("PackingItems");

        modelBuilder.Entity<Trip>()
            .HasMany(trip => trip.PackingItems)
            .WithOne(item => item.Trip)
            .HasForeignKey(item => item.TripId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}