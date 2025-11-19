using System.Text.Json;
using FluentAssertions;
using Nubrio.Infrastructure.OpenMeteo.OpenMeteoGeocoding.DTOs;

namespace Nubrio.Tests.Infrastructure.UnitTests.OpenMeteo.OpenMeteoGeocodingTests;

public class DeserializationResultTest
{
    [Fact]
    public void DeserializationResult_ShouldDeserialize()
    {
        // 1. Получаем текущую сборку тестов (в ней лежит embedded resource)
        var assembly = typeof(DeserializationResultTest).Assembly;

        // 2. Находим имя ресурса (он всегда строится как:
        //    <DefaultNamespace>.<папки через точки>.<имя_файла>)
        const string resourceName =
            "Nubrio.Tests.Infrastructure.UnitTests.OpenMeteo.TestData.OpenMeteoGeocodingTestData.geocoding-sample-en.json";

        // 3. Получаем поток ресурса
        using var stream = assembly.GetManifestResourceStream(resourceName);
        stream.Should().NotBeNull($"Embedded resource '{resourceName}' not found. Check Build Action and namespace.");

        using var reader = new StreamReader(stream!);
        var json = reader.ReadToEnd();
        
        var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
        
        var dto = JsonSerializer.Deserialize<OpenMeteoGeocodingResponse>(json, options);
        
        dto.Should().NotBeNull();
        dto.Results.Should().NotBeNull();
        dto.Results[0].Name.Should().Be("Berlin");
        dto.Results[0].Extra.Should().NotBeNull();
        dto.Results[0].Latitude.Should().BeApproximately(52.52437, 1e-5);
        dto.Results[0].Longitude.Should().BeApproximately(13.41053, 1e-5);
    }
}