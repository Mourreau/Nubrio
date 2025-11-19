using System.Net;
using FluentAssertions;
using Nubrio.Infrastructure.Http.GeocodingClient;
using Nubrio.Infrastructure.OpenMeteo.Validators.Errors;

namespace Nubrio.Tests.Infrastructure.IntegrationTests.Http;

public class OpenMeteoGeocodingClientTests
{
    [Fact]
    public async Task GeocodeAsync_ValidJson_ReturnsOk()
    {
        // 1. Получаем текущую сборку тестов 
        var assembly = typeof(OpenMeteoGeocodingClientTests).Assembly;

        // 2. Находим имя ресурса
        const string resourceName =
            "Nubrio.Tests.Infrastructure.UnitTests.OpenMeteo.TestData.OpenMeteoGeocodingTestData.geocoding-sample-en.json";

        // 3. Получаем поток ресурса
        using var stream = assembly.GetManifestResourceStream(resourceName);
        stream.Should().NotBeNull($"Embedded resource '{resourceName}' not found. Check Build Action and namespace.");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var handler = new StubHttpMessageHandler((req, _) =>
        {
            req.RequestUri!.PathAndQuery.Should().StartWith("/v1/search?");
            req.RequestUri.Query.Should().Contain("count=5");
            req.RequestUri.Query.Should().Contain("language=en");

            // NB: позже добавим проверку экранирования: name=New%20York

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            return responseMessage;
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://geocoding-api.open-meteo.com/") };

        var geoClient = new OpenMeteoGeocodingClient(client);

        var result = await geoClient.GeocodeAsync("Berlin", 5, "en", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Results.Should().NotBeNull().And.HaveCountGreaterThan(0);

        var dto = result.Value.Results[0];

        dto.Name.Should().Be("Berlin");
        dto.Id.Should().Be(2950159);
        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task GeocodeAsync_InvalidJson_ReturnsDeserializationError()
    {
        var invalid = "{ \"results\": [ { \"latitude\": \"oops\" } ] }"; // число как строка

        var handler = new StubHttpMessageHandler((_, _) =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(invalid) });

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://geocoding-api.open-meteo.com/") };
        var sut = new OpenMeteoGeocodingClient(client);

        var res = await sut.GeocodeAsync("X", 1, "en", CancellationToken.None);

        res.IsFailed.Should().BeTrue();
        res.Errors.Should().Contain(e => (string)e.Metadata["Code"] == OpenMeteoErrorCodes.Deserialization);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, OpenMeteoErrorCodes.Http5xx)]
    [InlineData((HttpStatusCode)429, OpenMeteoErrorCodes.TooManyRequests)]
    public async Task GeocodeAsync_5xxOr429_ReturnsProperCode(HttpStatusCode code, string expected)
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(code));
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://geocoding-api.open-meteo.com/") };
        var geoClient = new OpenMeteoGeocodingClient(client);

        var res = await geoClient.GeocodeAsync("City", 1, "en", CancellationToken.None);

        res.IsFailed.Should().BeTrue();
        res.Errors.Should().Contain(e => (string)e.Metadata["Code"] == expected);
    }

    internal sealed class ThrowingHandler : DelegatingHandler
    {
        private readonly Func<Exception> _throw;
        public ThrowingHandler(Func<Exception> @throw) => _throw = @throw;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromException<HttpResponseMessage>(_throw());
    }

    [Fact]
    public async Task GeocodeAsync_Timeout_ReturnsTimeoutCode()
    {
        var handler = new ThrowingHandler(() => new TaskCanceledException());
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://geocoding-api.open-meteo.com/") };

        var sut = new OpenMeteoGeocodingClient(client);
        var res = await sut.GeocodeAsync("City", 1, "en", CancellationToken.None);

        res.IsFailed.Should().BeTrue();
        res.Errors.Should().Contain(e => (string)e.Metadata["Code"] == OpenMeteoErrorCodes.Timeout);
    }

    [Fact]
    public async Task GeocodeAsync_NetworkError_ReturnsNetworkErrorCode()
    {
        var handler = new ThrowingHandler(() => new HttpRequestException("boom"));
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://geocoding-api.open-meteo.com/") };

        var sut = new OpenMeteoGeocodingClient(client);
        var res = await sut.GeocodeAsync("City", 1, "en", CancellationToken.None);

        res.IsFailed.Should().BeTrue();
        res.Errors.Should().Contain(e => (string)e.Metadata["Code"] == OpenMeteoErrorCodes.NetworkError);
    }

    [Fact]
    public async Task GeocodeAsync_EncodesCityProperly()
    {
        var handler = new StubHttpMessageHandler((req, _) =>
        {
            req.RequestUri!.Query.Should().Contain("name=New%20York");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"results\":[]}") };
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://geocoding-api.open-meteo.com/") };
        var sut = new OpenMeteoGeocodingClient(client);

        await sut.GeocodeAsync("New York", 5, "en", CancellationToken.None);
    }
}