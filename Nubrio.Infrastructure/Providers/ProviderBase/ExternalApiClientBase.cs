using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Microsoft.Extensions.Options;
using Nubrio.Infrastructure.Helpers.Errors.Extensions;
using Nubrio.Infrastructure.Options;
using Nubrio.Infrastructure.Providers.ProviderBase.ErrorsCodes;

namespace Nubrio.Infrastructure.Providers.ProviderBase;

internal abstract class ExternalApiClientBase
{
    public ProviderInfo Info { get; }
    protected ProviderErrorCodes ErrorCodes { get; }
    protected HttpClient HttpClient { get; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    protected ExternalApiClientBase(
        HttpClient httpClient,
        ProviderInfo providerInfo,
        ProviderErrorCodes errorCodes)
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
                    request.RequestUri!,
                    ErrorCodes.TooManyRequests(),
                    ErrorCodes.InternalError());

            await using var s = await response.Content.ReadAsStreamAsync(ct);

            var dto = await JsonSerializer.DeserializeAsync<TDto>(s, JsonOptions, ct);
            if (dto is null)
                return BuildError(
                    "Deserialization returned null",
                    request.RequestUri!,
                    ErrorCodes.DeserializationNull());

            return Result.Ok(dto);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            return BuildExceptionError(
                "Timeout",
                request.RequestUri!,
                ErrorCodes.Timeout(),
                ex);
        }
        catch (HttpRequestException ex)
        {
            return BuildExceptionError(
                "Network error",
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
        Uri requestUri,
        string tooManyRequestsCode,
        string http5xxCode)
    {
        var code = response.StatusCode == HttpStatusCode.TooManyRequests
            ? tooManyRequestsCode
            : http5xxCode;

        return new Error(
                $"External provider '{Info.Name}' responded {response.StatusCode} for {requestUri}")
            .WithProviderContext(Info,
                requestUri,
                statusCode: (int)response.StatusCode,
                providerErrorMessage: response.ReasonPhrase)
            .WithCode(code);
    }

    private Error BuildExceptionError(string message, Uri requestUri, string code, Exception ex)
        => new Error(message)
            .CausedBy(ex)
            .WithProviderContext(Info, requestUri)
            .WithCode(code);

    private Error BuildJsonExceptionError(Uri requestUri, string code, JsonException ex)
        => new Error("Deserialization failed")
            .CausedBy(ex)
            .WithProviderContext(Info, requestUri)
            .WithCode(code)
            .WithJsonException(ex);

}