namespace Nubrio.Tests.Infrastructure.IntegrationTests.Clients;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, int, HttpResponseMessage> _responder;
    public int Calls { get; private set; }

    public StubHttpMessageHandler(Func<HttpRequestMessage, int, HttpResponseMessage> responder)
    {
        _responder = responder;
    }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Calls++;
        var response = _responder(request, Calls);
        return Task.FromResult(response);
    }
}