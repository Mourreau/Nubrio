using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Microsoft.Extensions.Options;
using Nubrio.Infrastructure.Helpers.Errors.Extensions;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Infrastructure.Providers.ProviderBase;

internal abstract class ExternalApiClientBase<TErrorCodes>
    where TErrorCodes : ProviderErrorCodes
{
    private ProviderInfo Info { get; }
    protected TErrorCodes ErrorCodes { get; }
    protected HttpClient HttpClient { get; }

    private static readonly JsonSerializerOptions JsonOptions = new()
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

            var dto = await JsonSerializer.DeserializeAsync<TDto>(s, JsonOptions, ct);
            if (dto is null)
                return BuildError(
                    "Failed to deserialize response from external provider.",
                    request.RequestUri!,
                    ErrorCodes.DeserializationNull());

            return Result.Ok(dto);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return BuildExceptionError(
                "Timeout while calling external provider.",
                request.RequestUri!,
                ErrorCodes.Timeout(),
                ex);
        }
        catch (HttpRequestException ex)
        {
            return BuildExceptionError(
                "Network error while calling external provider.",
                request.RequestUri!,
                ErrorCodes.NetworkError(),
                ex);
        }
        catch (JsonException ex)
        {
            return BuildJsonExceptionError(
                request.RequestUri!,
                ErrorCodes.DeserializationException(),
                ex);
        }
    }

    protected Error BuildError(string message, Uri requestUri, string code)
        => new Error(message)
            .WithProviderContext(Info, requestUri)
            .WithCode(code);


    private Error BuildHttpError(HttpResponseMessage response,
        Uri requestUri)
    {
        var errorCode = response.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => ErrorCodes.TooManyRequests(),
            >= HttpStatusCode.InternalServerError => ErrorCodes.InternalError(),
            >= HttpStatusCode.BadRequest => ErrorCodes.ExternalClientError(),
            _ => ErrorCodes.InternalError()
        };

        return new Error(
                "External provider returned non-success status code.")
            .WithProviderContext(
                Info,
                requestUri,
                statusCode: (int)response.StatusCode,
                providerErrorMessage: response.ReasonPhrase)
            .WithCode(errorCode);
    }

    private Error BuildExceptionError(string message, Uri requestUri, string code, Exception ex)
        => new Error(message)
            .CausedBy(ex)
            .WithProviderContext(Info, requestUri)
            .WithCode(code);

    private Error BuildJsonExceptionError(Uri requestUri, string code, JsonException ex)
        => new Error("Failed to deserialize response from external provider.")
            .CausedBy(ex)
            .WithProviderContext(Info, requestUri)
            .WithCode(code)
            .WithJsonException(ex);
}