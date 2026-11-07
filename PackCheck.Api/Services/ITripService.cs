using PackCheck.Contracts;

namespace PackCheck.Api.Services;

public interface ITripService
{
    Task<List<TripDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TripDto> GetAsync(int id, CancellationToken cancellationToken);
    Task<TripDto> CreateAsync(CreateTripRequest request, CancellationToken cancellationToken);
    Task<TripDto> UpdateAsync(int id, UpdateTripRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
