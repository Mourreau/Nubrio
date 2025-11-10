using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Infrastructure.MockProvider.MockGeocoding;

public class MockGeocodingProvider : IGeocodingProvider
{
    public Task<Result<Location>> ResolveAsync(string city, string language, CancellationToken cancellationToken)
    {
        var location =  new Location(
            Guid.NewGuid(),
            city,
            new Coordinates(50, 60),
            "Asia/Yekaterinburg");
        
        return Task.FromResult(Result.Ok(location));
    }
}