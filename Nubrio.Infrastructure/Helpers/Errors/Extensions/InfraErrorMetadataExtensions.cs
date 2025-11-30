using System.Text.Json;
using FluentResults;
using Nubrio.Infrastructure.Providers.ProviderBase;

namespace Nubrio.Infrastructure.Helpers.Errors.Extensions;

/// <summary>
/// Набор расширений для удобного добавления метаданных в инфраструктурные ошибки.
/// Оборачивает FluentResults <see cref="Error"/> и дополняет его данными,
/// которые помогают отладке: каким провайдером был вызван процесс, какой URI использовался и т.д.
/// </summary>
internal static class InfraErrorMetadataExtensions
{
    public static Error WithCode(this Error error, string code)
        => error.WithMetadata(ProviderErrorMetadataKeys.Code, code);

    public static Error WithJsonException(this Error error, JsonException ex)
        => error.WithMetadata("ExceptionMessage", ex.Message)
            .WithMetadata("ExceptionPath", ex.Path);

    public static Error WithProviderContext(this Error error,
        ProviderInfo providerInfo,
        Uri uri,
        int? statusCode = null,
        string? providerErrorMessage = null)
    {
        var providerMetadata = new ProviderErrorMetadata
        (
            Name: providerInfo.Name, // Имя провайдера
            Service: providerInfo.Service, // Сервис, который был вызван
            Uri: uri.ToString(), // Запрос, который ушёл наружу
            StatusCode: statusCode,
            ProviderErrorMessage: providerErrorMessage
        );

        return error.WithMetadata(ProviderErrorMetadataKeys.Provider, providerMetadata);
    }
}