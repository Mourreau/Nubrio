using FluentResults;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IGeocodingProvider
{
    Task<Result<Location>> ResolveAsync(string city, string language, CancellationToken cancellationToken);
}