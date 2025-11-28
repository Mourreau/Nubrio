namespace Nubrio.Infrastructure.Helpers.Errors;

public sealed record ProviderErrorMetadata(
    string Name, 
    string Service,
    string Uri,
    int? StatusCode = null,
    string? ProviderErrorMessage = null);