using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces.Repository;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id, CancellationToken ct);
    
    Task<Location?> FindByExternalIdAsync(ExternalLocationId externalLocationId, CancellationToken ct);
    
    Task AddAsync(Location location, CancellationToken ct);
    
}