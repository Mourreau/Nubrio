using Nubrio.Application.Common;

namespace Nubrio.Presentation.Common;

public class HttpContextCacheHitAccessor : ICacheHitAccessor
{
    private const string Key = "__nubrio_cacheHit";
    private readonly IHttpContextAccessor _http;

    public HttpContextCacheHitAccessor(IHttpContextAccessor http)
    {
        _http = http;
    }


    public void SetCacheHit(bool isHit)
    {
        var context = _http.HttpContext;
        if (context is null) return;
        context.Items[Key] = isHit;
    }

    public bool? GetCacheHit()
    {
        var context = _http.HttpContext;
        if (context is null) return null;

        if (!context.Items.TryGetValue(Key, out var value)) return null;
        
        return value as bool? ?? (value is bool b ? b : null);
    }
}