using FluentResults;
using Nubrio.Application.Interfaces;
using Nubrio.Domain.Models;

namespace Nubrio.Infrastructure.Providers.MockProvider.MockGeocoding;

public class MockGeocodingProvider : IGeocodingProvider
{
    public MockGeocodingProvider()
    {
        Name = nameof(MockGeocodingProvider);
    }

    public Task<Result<Location>> ResolveAsync(string city, string language, CancellationToken cancellationToken)
    {
        var location =  new Location(
            Guid.NewGuid(),
            city,
            new Coordinates(50, 60),
            "Asia/Yekaterinburg",
            new ExternalLocationId(
                "MockGeocodingProvider",
                "002002"));
        
        return Task.FromResult(Result.Ok(location));
    }

    public string Name { get; }
}