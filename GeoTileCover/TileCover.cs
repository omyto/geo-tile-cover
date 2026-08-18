using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GeoTileCover;

/// <summary>Computes Web Mercator XYZ tile coverage for WGS84 geometries.</summary>
public static class TileCover
{
    /// <summary>The lowest supported XYZ zoom level.</summary>
    public const int MinZoom = 0;

    /// <summary>The highest supported XYZ zoom level.</summary>
    public const int MaxZoom = 25;

    /// <summary>Gets tiles covered by a WGS84 geometry at one zoom level from 0 through 25.</summary>
    public static IEnumerable<TileId> GetTiles(Geometry geometry, int zoom) => GetTiles(geometry, zoom, zoom);

    /// <summary>Gets tiles covered by a WGS84 geometry at every zoom level in the inclusive range from 0 through 25.</summary>
    public static IEnumerable<TileId> GetTiles(Geometry geometry, int minZoom, int maxZoom)
    {
        if (geometry == null)
        {
            throw new ArgumentNullException(nameof(geometry));
        }

        TileMath.TilesPerAxis(minZoom);
        TileMath.TilesPerAxis(maxZoom);
        if (maxZoom < minZoom)
        {
            throw new ArgumentOutOfRangeException(nameof(maxZoom), "maxZoom cannot be less than minZoom.");
        }

        if (geometry.SRID != 0 && geometry.SRID != 4326)
        {
            throw new ArgumentException("Geometry SRID must be 0 or 4326.", nameof(geometry));
        }

        if (geometry.IsEmpty)
        {
            yield break;
        }

        foreach (var tile in TileCoverAlgorithm.GetTiles(geometry, minZoom, maxZoom))
        {
            yield return tile;
        }
    }
}
