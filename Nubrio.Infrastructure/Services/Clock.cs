using Nubrio.Application.Interfaces;

namespace Nubrio.Infrastructure.Services;

public class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}