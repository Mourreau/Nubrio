using Nubrio.Application.Interfaces;

namespace Nubrio.Infrastructure.Services;

public class FetchTimeProvider : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.Now;
}