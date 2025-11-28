namespace Nubrio.Infrastructure.Providers.OpenMeteo.Validators;

public class ValidationResult(
    string message, 
    bool isSuccess, 
    bool isFailed, 
    int weatherElements = 0,
    string? code = null)
{
    public string Message { get; } = message;
    public bool IsSuccess { get; } = isSuccess;
    public bool IsFailed { get; } = isFailed;
    public int WeatherElements { get; } = weatherElements;
    public string? Code { get; } = code;
}