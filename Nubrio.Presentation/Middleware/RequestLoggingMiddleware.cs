using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Nubrio.Application.Common;
using Nubrio.Application.Interfaces;
using Nubrio.Infrastructure.Telemetry;
using Nubrio.Presentation.Controllers;

namespace Nubrio.Presentation.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private const string DailyActionName = nameof(WeatherController.GetDailyForecastByCity);
    private const string WeeklyActionName = nameof(WeatherController.GetWeeklyForecastByCity);

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IClock clock,
        IRequestLogStore logStore,
        ICacheHitAccessor accessor)
    {
        _logger.LogInformation("RequestLoggingMiddleware invoked");
        
        var timestampUtc = clock.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug("Finally block with TIME: {Time}", stopwatch.ElapsedMilliseconds);
        }

        var statusCode = context.Response.StatusCode;
        var latencyMs = stopwatch.ElapsedMilliseconds;
        var cityRaw = context.Request.RouteValues["city"] as string;

        if (string.IsNullOrEmpty(cityRaw))
            return;

        var cityNormalized = cityRaw.ToLowerInvariant().Trim();
        DateOnly? date = null;
        var isDay = context.Request.Query.TryGetValue("date", out var dateString);
        var endpoint = string.Empty;

        var endpointAction = context.GetEndpoint();

        if (endpointAction is null)
            return;

        var actionMeta = endpointAction.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionMeta is null) return;
        var actionName = actionMeta.ActionName;


        switch (actionName)
        {
            case WeeklyActionName:
                endpoint = "week";
                _logger.LogDebug("Endpoint: 'Week'");
                break;
            case DailyActionName when !isDay:
                return;
            case DailyActionName:
            {
                if (!DateOnly.TryParse(dateString[0], out var parsed))
                    return;

                date = parsed;
                endpoint = "day";
                _logger.LogDebug("Endpoint: 'Day'");
                break;
            }
        }

        if (string.IsNullOrEmpty(endpoint))
            return;


        var cacheHit = accessor.GetCacheHit();

        if (cacheHit is null)
            _logger.LogDebug("CacheHit is null: cache was not evaluated for this request");


        if (cacheHit == false)
            _logger.LogDebug("CacheHit is false (cache miss). There is no cache stored");


        var entry = new RequestLogEntry(
            TimestampUtc: timestampUtc,
            City: cityNormalized,
            Endpoint: endpoint,
            Date: date,
            CacheHit: cacheHit,
            StatusCode: statusCode,
            LatencyMs: (int)latencyMs
        );

        try
        {
            await logStore.LogAsync(entry, context.RequestAborted);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to log request");
        }
    }
}

public static class NubrioMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}