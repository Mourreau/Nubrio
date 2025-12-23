namespace Nubrio.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}