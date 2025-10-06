using Nubrio.Domain.Models;

namespace Nubrio.Application.Interfaces;

public interface IGeocodingService
{
    Location Resolve(string city);
}