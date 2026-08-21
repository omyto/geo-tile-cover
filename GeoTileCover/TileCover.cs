using System;
using System.Collections.Generic;
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
        TileMath.TilesPerAxis(minZoom);
        TileMath.TilesPerAxis(maxZoom);
        if (maxZoom < minZoom)
        {
            throw new ArgumentOutOfRangeException(nameof(maxZoom), "maxZoom cannot be less than minZoom.");
        }

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
}
