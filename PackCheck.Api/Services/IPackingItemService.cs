using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public interface IPackingItemService
{
    Task<List<PackingItemDto>> GetAllAsync(int tripId, CancellationToken cancellationToken);
    Task<PackingItemDto> GetAsync(int tripId, int itemId, CancellationToken cancellationToken);
    Task<PackingItemDto> CreateAsync(int tripId, SavePackingItemRequest request, CancellationToken cancellationToken);
    Task<PackingItemDto> UpdateAsync(int tripId, int itemId, SavePackingItemRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int tripId, int itemId, CancellationToken cancellationToken);
}
