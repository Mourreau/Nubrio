namespace Nubrio.Application.Common.Errors;

public enum AppErrorCode
{
    EmptyCity,
    DateOutOfRange,
    LocationNotFound,
    ForecastNotFound,
    ExternalClientError,   // 4xx от провайдера 
    ExternalServerError,   // 5xx от провайдера
    ProviderBadResponse,
    TooManyRequests,   
    Timeout,
    Unknown
}