using FluentResults;

namespace Nubrio.Application.Common.Errors;

public static class ExternalErrorMetadataExtensions
{
    public static bool TryGetAppErrorCode(this IError error, out AppErrorCode appErrorCode)
    {
        appErrorCode = AppErrorCode.Unknown;

        if (!error.Metadata.TryGetValue(ProviderErrorMetadataKeys.ServiceCode, out var value))
            return false;

        if (value is AppErrorCode typed)
        {
            appErrorCode = typed;
            return true;
        }

        if (value is string str && Enum.TryParse<AppErrorCode>(str, out var parsedCode))
        {
            appErrorCode = parsedCode;
            return true;
        }
        return false;
    }

    public static bool TryGetProviderCode(this IError error, out string? providerCode)
    {
        providerCode = null;

        if (!error.Metadata.TryGetValue(ProviderErrorMetadataKeys.ProviderCode, out var value))
            return false;

        if (value is string code )
        {
            providerCode = code;
            return true;
        }
            
        return false;
    }
}