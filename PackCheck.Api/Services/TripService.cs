using PackCheck.Api.Entities;
using PackCheck.Api.Infrastructure;
using PackCheck.Api.Repositories;
using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public class TripService(ITripRepository repository) : ITripService
{
    public async Task<List<TripDto>> GetAllAsync(CancellationToken cancellationToken) =>
        (await repository.GetAllAsync(cancellationToken)).Select(DtoMapping.ToDto).ToList();

    public async Task<TripDto> GetAsync(int id, CancellationToken cancellationToken) =>
        (await FindAsync(id, cancellationToken)).ToDto();

    public async Task<TripDto> CreateAsync(CreateTripRequest request, CancellationToken cancellationToken)
    {
        var template = PackingTemplates.All.SingleOrDefault(template => template.Key == request.TemplateKey);
        if (template is null)
            throw new ApiException(400, "Choose a valid packing template.");

        var trip = new Trip
        {
            Name = ValidName(request.Name),
            DepartureDate = request.DepartureDate.Date,
            TripType = template.Name,
            Items = template.Items.Select(item => new PackingItem
            {
                Name = item.Name,
                CategoryKey = item.CategoryKey,
                RequiredQuantity = item.RequiredQuantity,
                IsEssential = item.IsEssential
            }).ToList()
        };
        await repository.AddAsync(trip, cancellationToken);
        return trip.ToDto();
    }

    public async Task<TripDto> UpdateAsync(int id, UpdateTripRequest request, CancellationToken cancellationToken)
    {
        var trip = await FindAsync(id, cancellationToken);
        trip.Name = ValidName(request.Name);
        trip.DepartureDate = request.DepartureDate.Date;
        await repository.SaveChangesAsync(cancellationToken);
        return trip.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken) =>
        await repository.DeleteAsync(await FindAsync(id, cancellationToken), cancellationToken);

    private async Task<Trip> FindAsync(int id, CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken) ?? throw new ApiException(404, "This trip no longer exists.");

    internal static string ValidName(string? name)
    {
        var trimmedName = name?.Trim();
        if (string.IsNullOrEmpty(trimmedName) || trimmedName.Length > 100)
            throw new ApiException(400, "Enter a name between 1 and 100 characters.");
        return trimmedName;
    }
}
