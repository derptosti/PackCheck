using PackCheck.Api.Entities;

namespace PackCheck.Api.Repositories;

public interface ITripRepository
{
    Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken);
    Task<Trip?> GetAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Trip trip, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Trip trip, CancellationToken cancellationToken);
}
