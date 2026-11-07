using PackCheck.Api.Entities;
using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public static class DtoMapping
{
    public static TripDto ToDto(this Trip trip) => new()
    {
        Id = trip.Id,
        Name = trip.Name,
        DepartureDate = trip.DepartureDate,
        TripType = trip.TripType,
        Items = trip.Items.OrderByDescending(item => item.IsEssential).ThenBy(item => item.Id).Select(ToDto).ToList()
    };

    public static PackingItemDto ToDto(this PackingItem item) => new()
    {
        Id = item.Id,
        TripId = item.TripId,
        Name = item.Name,
        CategoryKey = item.CategoryKey,
        RequiredQuantity = item.RequiredQuantity,
        PackedQuantity = item.PackedQuantity,
        IsEssential = item.IsEssential
    };
}
