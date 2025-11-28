namespace Nubrio.Infrastructure.Providers.OpenMeteo.Validators.Errors;

public static class OpenMeteoErrorCodes
{
    public const string MalformedDailyMean = "OpenMeteo.MalformedDailyMean";
    public const string UnitsMismatch      = "OpenMeteo.UnitsMismatch";
    public const string Http5xx            = "OpenMeteo.Http5xx";
    public const string TooManyRequests    = "OpenMeteo.TooManyRequests";
    public const string Timeout            = "OpenMeteo.Timeout";
    public const string NetworkError       = "OpenMeteo.NetworkError";
    public const string Deserialization    = "OpenMeteo.Deserialization";
    public const string GeocodingNotFound = "OpenMeteo.GeocodingNotFound";
}
// TODO: УДАЛИТЬ!!!