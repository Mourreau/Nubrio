using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Microsoft.Extensions.Options;
using Nubrio.Application.Common.Errors;
using Nubrio.Infrastructure.Helpers.Errors.Extensions;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Infrastructure.Providers.ProviderBase;

internal abstract class ExternalApiClientBase<TErrorCodes>
    where TErrorCodes : ProviderErrorCodes
{
    protected ProviderInfo Info { get; }
    protected TErrorCodes ErrorCodes { get; }
    protected HttpClient HttpClient { get; }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    protected ExternalApiClientBase(
        HttpClient httpClient,
        ProviderInfo providerInfo,
        TErrorCodes errorCodes)
    {
        HttpClient = httpClient;
        Info = providerInfo;
        ErrorCodes = errorCodes;
    }

    protected async Task<Result<TDto>> SendAndDeserializeAsync<TDto>(
        HttpRequestMessage request,
        CancellationToken ct
    )
    {
        try
        {
            using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
                return BuildHttpError(response,
                    request.RequestUri!);

            await using var s = await response.Content.ReadAsStreamAsync(ct);

            var dto = await JsonSerializer.DeserializeAsync<TDto>(s, _jsonOptions, ct);
            if (dto is null)
                return BuildError(
                    "Failed to deserialize response from external provider.",
                    request.RequestUri!,
                    ErrorCodes.DeserializationNull(),
                    AppErrorCode.ProviderBadResponse);

            return Result.Ok(dto);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return BuildExceptionError(
                "Timeout while calling external provider.",
                request.RequestUri!,
                ErrorCodes.Timeout(),
                AppErrorCode.Timeout,
                ex);
        }
        catch (HttpRequestException ex)
        {
            return BuildExceptionError(
                "Network error while calling external provider.",
                request.RequestUri!,
                ErrorCodes.NetworkError(),
                AppErrorCode.ExternalServerError,
                ex);
        }
        catch (JsonException ex)
        {
            return BuildJsonExceptionError(
                request.RequestUri!,
                ErrorCodes.DeserializationException(),
                AppErrorCode.ProviderBadResponse,
                ex);
        }
    }

    protected Error BuildError(string message, Uri requestUri, string providerCode, AppErrorCode serviceCode) =>
        new Error(message)
            .WithProviderContext(Info, requestUri)
            .WithCodes(providerCode, serviceCode);


    private Error BuildHttpError(HttpResponseMessage response,
        Uri requestUri)
    {
        var (providerCode, serviceCode) = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests =>
                (ErrorCodes.TooManyRequests(), AppErrorCode.TooManyRequests),
            >= HttpStatusCode.InternalServerError =>
                (ErrorCodes.InternalError(), AppErrorCode.ExternalServerError),
            >= HttpStatusCode.BadRequest =>
                (ErrorCodes.ExternalClientError(), AppErrorCode.ExternalClientError),
            _ => (ErrorCodes.InternalError(), AppErrorCode.Unknown)
        };

        return new Error(
                "External provider returned non-success status code.")
            .WithProviderContext(
                Info,
                requestUri,
                statusCode: (int)response.StatusCode,
                providerErrorMessage: response.ReasonPhrase)
            .WithCodes(providerCode, serviceCode);
    }

    private Error BuildExceptionError(
        string message,
        Uri requestUri,
        string providerCode,
        AppErrorCode serviceCode,
        Exception ex) =>
        new Error(message)
            .CausedBy(ex)
            .WithProviderContext(Info, requestUri)
            .WithCodes(providerCode, serviceCode);

    private Error BuildJsonExceptionError(Uri requestUri, string providerCode, AppErrorCode serviceCode,
        JsonException ex) =>
        new Error("Failed to deserialize response from external provider.")
            .CausedBy(ex)
            .WithProviderContext(Info, requestUri)
            .WithCodes(providerCode, serviceCode)
            .WithJsonException(ex);
}