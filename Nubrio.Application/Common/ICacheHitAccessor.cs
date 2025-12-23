namespace Nubrio.Application.Common;

public interface ICacheHitAccessor
{
    void SetCacheHit(bool isHit);
    bool? GetCacheHit();
}