using FluentResults;
using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IGeocodingService
{
    Task<Result<Location>> ResolveAsync(string city, CancellationToken cancellationToken);
}