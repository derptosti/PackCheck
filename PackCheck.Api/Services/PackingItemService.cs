using PackCheck.Api.Entities;
using PackCheck.Api.Infrastructure;
using PackCheck.Api.Repositories;
using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public class PackingItemService(IPackingItemRepository repository, ITripRepository trips) : IPackingItemService
{
    public async Task<List<PackingItemDto>> GetAllAsync(int tripId, CancellationToken cancellationToken)
    {
        await EnsureTripExistsAsync(tripId, cancellationToken);
        return (await repository.GetAllAsync(tripId, cancellationToken)).Select(DtoMapping.ToDto).ToList();
    }

    public async Task<PackingItemDto> GetAsync(int tripId, int itemId, CancellationToken cancellationToken) =>
        (await FindAsync(tripId, itemId, cancellationToken)).ToDto();

    public async Task<PackingItemDto> CreateAsync(int tripId, SavePackingItemRequest request, CancellationToken cancellationToken)
    {
        await EnsureTripExistsAsync(tripId, cancellationToken);
        var item = new PackingItem { TripId = tripId };
        ApplyRequest(item, request);
        await repository.AddAsync(item, cancellationToken);
        return item.ToDto();
    }

    public async Task<PackingItemDto> UpdateAsync(int tripId, int itemId, SavePackingItemRequest request, CancellationToken cancellationToken)
    {
        var item = await FindAsync(tripId, itemId, cancellationToken);
        ApplyRequest(item, request);
        await repository.SaveChangesAsync(cancellationToken);
        return item.ToDto();
    }

    public async Task DeleteAsync(int tripId, int itemId, CancellationToken cancellationToken) =>
        await repository.DeleteAsync(await FindAsync(tripId, itemId, cancellationToken), cancellationToken);

    private async Task<PackingItem> FindAsync(int tripId, int itemId, CancellationToken cancellationToken) =>
        await repository.GetAsync(tripId, itemId, cancellationToken)
            ?? throw new ApiException(404, "This packing item does not exist in the selected trip.");

    private async Task EnsureTripExistsAsync(int tripId, CancellationToken cancellationToken)
    {
        if (!await trips.ExistsAsync(tripId, cancellationToken))
            throw new ApiException(404, "This trip no longer exists.");
    }

    private static void ApplyRequest(PackingItem item, SavePackingItemRequest request)
    {
        var name = TripService.ValidName(request.Name);
        var categoryKey = string.IsNullOrWhiteSpace(request.CategoryKey) ? null : request.CategoryKey.Trim();
        if (categoryKey is not null && !ItemCategories.IsSupported(categoryKey))
            throw new ApiException(400, "Choose a supported scan category, or leave the category empty for a manual item.");
        if (request.RequiredQuantity is < 1 or > 999 || request.PackedQuantity < 0 || request.PackedQuantity > request.RequiredQuantity)
            throw new ApiException(400, "Required quantity must be 1–999. Packed quantity must be between zero and the required quantity.");

        item.Name = name;
        item.CategoryKey = categoryKey;
        item.RequiredQuantity = request.RequiredQuantity;
        item.PackedQuantity = request.PackedQuantity;
        item.IsEssential = request.IsEssential;
    }
}
