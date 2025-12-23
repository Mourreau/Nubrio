using Microsoft.EntityFrameworkCore;
using Nubrio.Application.Interfaces.Repository;
using Nubrio.Domain.Models;

namespace Nubrio.Infrastructure.Persistence.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly NubrioDbContext _context;

    public LocationRepository(NubrioDbContext context)
    {
        _context = context;
    }


    public async Task<Location?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var result = await _context.Locations.FirstOrDefaultAsync(x =>
                x.Id == id,
            cancellationToken: ct);

        return result;
    }

    public async Task<Location?> FindByExternalIdAsync(ExternalLocationId externalLocationId, CancellationToken ct)
    {
        var result =
            await _context.Locations.FirstOrDefaultAsync(x =>
                    x.ExternalLocationId.ProviderName == externalLocationId.ProviderName &&
                    x.ExternalLocationId.Value == externalLocationId.Value,
                cancellationToken: ct);

        return result;
    }

    public async Task AddAsync(Location location, CancellationToken ct)
    {
        await _context.Locations.AddAsync(location, ct);
    }
}