using Nubrio.Application.Interfaces;

namespace Nubrio.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}