using System;
using System.Collections.Generic;
using System.Threading;
using NetTopologySuite.Geometries;

namespace GeoTileCover;

/// <summary>Computes Web Mercator XYZ tile coverage for a WGS84 geometry or a union of fully covered tiles.</summary>
public sealed class TileCover
{
    /// <summary>The lowest supported XYZ zoom level.</summary>
    public const int MinZoom = 0;

    /// <summary>The highest supported XYZ zoom level.</summary>
    public const int MaxZoom = 25;

    private readonly object _cacheLock = new object();
    private readonly Dictionary<int, TileId[]> _tilesByZoom = new Dictionary<int, TileId[]>();
    private readonly CanonicalTileUnion? _tileUnion;
    private readonly Geometry? _geometry;

    /// <summary>Creates a tile cover for a WGS84 geometry.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="geometry"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A geometry coordinate is outside the supported range.</exception>
    /// <exception cref="ArgumentException">The geometry SRID is neither 0 nor 4326.</exception>
    /// <exception cref="NotSupportedException">A geometry segment crosses the antimeridian and must be split first.</exception>
    public TileCover(Geometry geometry)
    {
        if (geometry == null)
        {
            throw new ArgumentNullException(nameof(geometry));
        }

        _geometry = (Geometry)geometry.Copy();
        if (_geometry.SRID != 0 && _geometry.SRID != 4326)
        {
            throw new ArgumentException("Geometry SRID must be 0 or 4326.", nameof(geometry));
        }

        if (!_geometry.IsEmpty)
        {
            GeometryValidator.Validate(_geometry);
        }
    }

    /// <summary>Creates a tile cover from the union of fully covered XYZ tiles.</summary>
    /// <remarks>Source tiles may use different zoom levels. Redundant descendants and complete groups of siblings are compacted without expanding tiles to a common zoom.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="tiles"/> is null.</exception>
    public TileCover(IEnumerable<TileId> tiles)
    {
        _tileUnion = new CanonicalTileUnion(tiles);
    }

    /// <summary>Gets tiles covered by this cover at one zoom level from 0 through 25.</summary>
    public IEnumerable<TileId> GetTiles(int zoom) => GetTiles(zoom, zoom);

    /// <summary>Gets tiles covered by this cover at every zoom level in the inclusive range from 0 through 25.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A zoom is outside the supported range or <paramref name="maxZoom"/> is less than <paramref name="minZoom"/>.</exception>
    public IEnumerable<TileId> GetTiles(int minZoom, int maxZoom)
    {
        ValidateZoomRange(minZoom, maxZoom);

        TileId[][] levels;
        lock (_cacheLock)
        {
            EnsureCached(minZoom, maxZoom);
            levels = new TileId[maxZoom - minZoom + 1][];
            for (var zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                levels[zoom - minZoom] = _tilesByZoom[zoom];
            }
        }

        return Enumerate(levels);
    }

    /// <summary>Attempts to get the complete tile cover at one zoom without exceeding a limit on distinct tiles.</summary>
    /// <param name="zoom">The zoom level from 0 through 25.</param>
    /// <param name="maxTiles">The maximum number of distinct tiles allowed, including zero for an empty cover.</param>
    /// <param name="tiles">The complete result ordered by row then column on success; an empty array when the limit is exceeded.</param>
    /// <param name="cancellationToken">Cancels cache waits and tile traversal cooperatively.</param>
    /// <returns>True if the complete result fits within the limit; otherwise, false.</returns>
    /// <remarks>Only complete results are cached. The returned array can be modified without affecting the cache. Cancellation is checked between traversal steps, not inside individual geometry operations.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is unsupported or maxTiles is negative.</exception>
    /// <exception cref="OperationCanceledException">Cancellation is requested.</exception>
    public bool TryGetTiles(int zoom, int maxTiles, out TileId[] tiles, CancellationToken cancellationToken = default)
    {
        TileMath.TilesPerAxis(zoom);
        if (maxTiles < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTiles));
        }

        tiles = Array.Empty<TileId>();
        var lockTaken = false;
        try
        {
            // Unlike lock, a bounded wait lets a disconnected caller cancel while another
            // request is computing tiles on this instance.
            while (!lockTaken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Monitor.TryEnter(_cacheLock, 50, ref lockTaken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (_tilesByZoom.TryGetValue(zoom, out var complete))
            {
                if (complete.Length > maxTiles)
                {
                    return false;
                }
            }
            else if (!TryComputeTiles(zoom, maxTiles, out complete, cancellationToken))
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = (TileId[])complete.Clone();
            cancellationToken.ThrowIfCancellationRequested();
            _tilesByZoom[zoom] = complete;
            tiles = snapshot;
            return true;
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(_cacheLock);
            }
        }
    }

    private bool TryComputeTiles(int zoom, int maxTiles, out TileId[] tiles, CancellationToken cancellationToken)
    {
        for (var sourceZoom = zoom + 1; sourceZoom <= MaxZoom; sourceZoom++)
        {
            if (_tilesByZoom.TryGetValue(sourceZoom, out var source))
            {
                // Apply the limit to the requested zoom, not to intermediate parent levels.
                return TileCoverAlgorithm.TryGetAncestorTiles(source, zoom, maxTiles, out tiles, cancellationToken);
            }
        }

        return _geometry != null
            ? TileCoverAlgorithm.TryGetTilesAtZoom(_geometry, zoom, maxTiles, out tiles, cancellationToken)
            : _tileUnion!.TryGetTiles(zoom, maxTiles, out tiles, cancellationToken);
    }

    /// <summary>Gets the smallest mixed-zoom tile set between two zoom levels that covers this cover at <paramref name="maxZoom"/> resolution.</summary>
    /// <remarks>Complete sibling groups are recursively replaced by their parent, stopping at <paramref name="minZoom"/>.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">A zoom is outside the supported range or <paramref name="maxZoom"/> is less than <paramref name="minZoom"/>.</exception>
    public TileId[] GetMinimalTiles(int minZoom, int maxZoom)
    {
        ValidateZoomRange(minZoom, maxZoom);
        if (_tileUnion != null)
        {
            return _tileUnion.GetMinimalTiles(minZoom, maxZoom);
        }

        TileId[] tilesAtMaxZoom;
        lock (_cacheLock)
        {
            EnsureCached(maxZoom, maxZoom);
            tilesAtMaxZoom = _tilesByZoom[maxZoom];
        }

        return new CanonicalTileUnion(tilesAtMaxZoom).GetMinimalTiles(minZoom, maxZoom);
    }

    private void EnsureCached(int minZoom, int maxZoom)
    {
        var sourceZoom = maxZoom;
        while (sourceZoom <= MaxZoom && !_tilesByZoom.ContainsKey(sourceZoom))
        {
            sourceZoom++;
        }

        if (sourceZoom > MaxZoom)
        {
            _tilesByZoom[maxZoom] = _geometry != null
                ? TileCoverAlgorithm.GetTilesAtZoom(_geometry, maxZoom)
                : _tileUnion!.GetTiles(maxZoom);
            sourceZoom = maxZoom;
        }

        for (var zoom = sourceZoom - 1; zoom >= minZoom; zoom--)
        {
            if (!_tilesByZoom.ContainsKey(zoom))
            {
                _tilesByZoom[zoom] = TileCoverAlgorithm.GetParentTiles(_tilesByZoom[zoom + 1]);
            }
        }
    }

    private static IEnumerable<TileId> Enumerate(TileId[][] levels)
    {
        foreach (var tiles in levels)
        {
            foreach (var tile in tiles)
            {
                yield return tile;
            }
        }
    }

    private static void ValidateZoomRange(int minZoom, int maxZoom)
    {
        TileMath.TilesPerAxis(minZoom);
        TileMath.TilesPerAxis(maxZoom);
        if (maxZoom < minZoom)
        {
            throw new ArgumentOutOfRangeException(nameof(maxZoom), "maxZoom cannot be less than minZoom.");
        }
    }
}
