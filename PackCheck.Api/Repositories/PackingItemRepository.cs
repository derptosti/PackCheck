using Microsoft.EntityFrameworkCore;
using PackCheck.Api.Data;
using PackCheck.Api.Entities;

namespace PackCheck.Api.Repositories;

public class PackingItemRepository(PackCheckDbContext context) : IPackingItemRepository
{
    public Task<List<PackingItem>> GetAllAsync(int tripId, CancellationToken cancellationToken) =>
        context.PackingItems.AsNoTracking().Where(item => item.TripId == tripId)
            .OrderByDescending(item => item.IsEssential).ThenBy(item => item.Id).ToListAsync(cancellationToken);

    public Task<PackingItem?> GetAsync(int tripId, int itemId, CancellationToken cancellationToken) =>
        context.PackingItems.SingleOrDefaultAsync(item => item.TripId == tripId && item.Id == itemId, cancellationToken);

    public async Task AddAsync(PackingItem item, CancellationToken cancellationToken)
    {
        await context.PackingItems.AddAsync(item, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await context.SaveChangesAsync(cancellationToken);

    public async Task DeleteAsync(PackingItem item, CancellationToken cancellationToken)
    {
        context.PackingItems.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
    }
}
