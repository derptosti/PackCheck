using Microsoft.EntityFrameworkCore;
using PackCheck.Api.Data;
using PackCheck.Api.Entities;

namespace PackCheck.Api.Repositories;

public class TripRepository(PackCheckDbContext context) : ITripRepository
{
    public Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken) =>
        context.Trips.AsNoTracking().Include(trip => trip.Items)
            .OrderBy(trip => trip.DepartureDate).ThenBy(trip => trip.Id).ToListAsync(cancellationToken);

    public Task<Trip?> GetAsync(int id, CancellationToken cancellationToken) =>
        context.Trips.Include(trip => trip.Items).SingleOrDefaultAsync(trip => trip.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        context.Trips.AnyAsync(trip => trip.Id == id, cancellationToken);

    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        await context.Trips.AddAsync(trip, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await context.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(Trip trip, CancellationToken cancellationToken)
    {
        context.Trips.Remove(trip);
        await context.SaveChangesAsync(cancellationToken);
    }
}
