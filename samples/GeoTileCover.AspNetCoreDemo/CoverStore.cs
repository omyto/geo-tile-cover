using Microsoft.Extensions.Caching.Memory;

namespace GeoTileCover.AspNetCoreDemo;

internal static class CoverOptions
{
    public const int MaxTilesPerResponse = 100_000;
    public static readonly TimeSpan ComputationTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);
}

internal sealed class CoverStore
{
    private readonly IMemoryCache _cache;
    private readonly TimeProvider _timeProvider;

    public CoverStore(IMemoryCache cache, TimeProvider timeProvider)
    {
        _cache = cache;
        _timeProvider = timeProvider;
    }

    public CachedCover Create(TileCover cover)
    {
        var cachedCover = new CachedCover(Guid.CreateVersion7(), cover, _timeProvider.GetUtcNow().Add(CoverOptions.Lifetime));
        _cache.Set(cachedCover.Id, cachedCover, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = cachedCover.ExpiresAt
        });

        return cachedCover;
    }

    public bool TryGet(Guid id, out CachedCover cachedCover)
    {
        if (_cache.TryGetValue(id, out CachedCover? value) && value is not null)
        {
            cachedCover = value;
            return true;
        }

        cachedCover = null!;
        return false;
    }
}

internal sealed record CachedCover(Guid Id, TileCover Cover, DateTimeOffset ExpiresAt);
