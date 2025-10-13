using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Infrastructure.MockProvider.MockGeocoding;

public class MockGeocodingService : IGeocodingService
{
    public Task<Result<Location>> ResolveAsync(string city, CancellationToken cancellationToken)
    {
        var location =  new Location(
            Guid.NewGuid(),
            city,
            new Coordinates(50, 60),
            "Asia");
        
        return Task.FromResult(Result.Ok(location));
    }
}