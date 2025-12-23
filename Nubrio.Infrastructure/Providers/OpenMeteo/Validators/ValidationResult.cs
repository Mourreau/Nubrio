namespace Nubrio.Infrastructure.Providers.OpenMeteo.Validators;

public sealed class ValidationResult(
    string message,
    bool isSuccess,
    int weatherElements = 0,
    string? code = null)
{
    public string Message { get; } = message;
    public bool IsSuccess { get; } = isSuccess;
    public bool IsFailed => !IsSuccess;
    public int WeatherElements { get; } = weatherElements;
    public string? Code { get; } = code;
}