using System.Collections.Concurrent;
using FluentResults;
using Nubrio.Application.Interfaces;

namespace Nubrio.Infrastructure.Services;

public class TimeZoneResolver : ITimeZoneResolver
{
    private static readonly ConcurrentDictionary<string, TimeZoneInfo> ZoneCache = new();
    
    public Result<TimeZoneInfo> GetTimeZoneInfo(string timeZoneId)
    {
        var normalizedId = Normalize(timeZoneId);
        
        if (string.IsNullOrWhiteSpace(normalizedId))
            return Result.Fail("Time zone Id cannot be null or empty");
        
        if(ZoneCache.TryGetValue(normalizedId, out var result))
            return Result.Ok(result);
        

        if(!TimeZoneInfo.TryFindSystemTimeZoneById(normalizedId, out var timeZoneInfo))
            return Result.Fail($"Cannot resolve time zone by id: {normalizedId}");
            
        
        ZoneCache.TryAdd(normalizedId, timeZoneInfo);
        
        return Result.Ok(timeZoneInfo);
    }

    private static string Normalize(string timeZoneId)
    {
        return timeZoneId.Trim();
    }
}