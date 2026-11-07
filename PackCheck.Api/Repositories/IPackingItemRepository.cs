using PackCheck.Api.Entities;

namespace PackCheck.Api.Repositories;

public interface IPackingItemRepository
{
    Task<List<PackingItem>> GetAllAsync(int tripId, CancellationToken cancellationToken);
    Task<PackingItem?> GetAsync(int tripId, int itemId, CancellationToken cancellationToken);
    Task AddAsync(PackingItem item, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task DeleteAsync(PackingItem item, CancellationToken cancellationToken);
}
