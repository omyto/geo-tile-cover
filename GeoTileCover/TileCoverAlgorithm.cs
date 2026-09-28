using System;
using System.Collections.Generic;
using System.Threading;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace GeoTileCover;

internal static class TileCoverAlgorithm
{
    public static TileId[] GetTilesAtZoom(Geometry geometry, int zoom)
    {
        TryGetTilesAtZoom(geometry, zoom, null, out var tiles);
        return tiles;
    }

    public static bool TryGetTilesAtZoom(Geometry geometry, int zoom, int? maxTiles, out TileId[] tiles, CancellationToken cancellationToken = default)
    {
        tiles = Array.Empty<TileId>();
        var result = new TileAccumulator(maxTiles, cancellationToken);
        if (!CollectAtZoom(geometry, zoom, result))
        {
            return false;
        }

        tiles = result.ToArray();
        return true;
    }

    public static TileId[] GetParentTiles(IEnumerable<TileId> tiles)
    {
        var parents = new TileAccumulator();
        foreach (var tile in tiles)
        {
            parents.TryAdd(tile.Parent());
        }

        return parents.ToArray();
    }

    public static bool TryGetAncestorTiles(IEnumerable<TileId> source, int zoom, int maxTiles, out TileId[] tiles, CancellationToken cancellationToken)
    {
        tiles = Array.Empty<TileId>();
        var result = new TileAccumulator(maxTiles, cancellationToken);
        foreach (var tile in source)
        {
            var shift = tile.Z - zoom;
            if (!result.TryAdd(new TileId(zoom, tile.X >> shift, tile.Y >> shift)))
            {
                return false;
            }
        }

        tiles = result.ToArray();
        return true;
    }

    private static bool CollectAtZoom(Geometry geometry, int zoom, TileAccumulator result)
    {
        result.CheckCancellation();
        if (geometry is GeometryCollection collection)
        {
            for (var index = 0; index < collection.NumGeometries; index++)
            {
                if (!CollectAtZoom(collection.GetGeometryN(index), zoom, result))
                {
                    return false;
                }
            }

            return true;
        }

        if (geometry.Dimension == Dimension.Point)
        {
            foreach (var coordinate in geometry.Coordinates)
            {
                if (!result.TryAdd(TileMath.ToTile(coordinate.X, coordinate.Y, zoom)))
                {
                    return false;
                }
            }

            return true;
        }

        var prepared = PreparedGeometryFactory.Prepare(geometry);
        return Visit(new TileId(0, 0, 0), zoom, geometry, prepared, result);
    }

    private static bool Visit(TileId tile, int targetZoom, Geometry source, IPreparedGeometry prepared, TileAccumulator result)
    {
        result.CheckCancellation();
        var tileGeometry = source.Factory.ToGeometry(TileMath.ToEnvelope(tile));
        if (!prepared.Intersects(tileGeometry))
        {
            return true;
        }

        if (tile.Z == targetZoom)
        {
            if (IntersectsTileInterior(source, tileGeometry) || LineBoundaryBelongsToTile(source, tileGeometry, tile))
            {
                return result.TryAdd(tile);
            }

            return true;
        }

        var z = tile.Z + 1;
        var x = tile.X << 1;
        var y = tile.Y << 1;
        return Visit(new TileId(z, x, y), targetZoom, source, prepared, result)
            && Visit(new TileId(z, x + 1, y), targetZoom, source, prepared, result)
            && Visit(new TileId(z, x, y + 1), targetZoom, source, prepared, result)
            && Visit(new TileId(z, x + 1, y + 1), targetZoom, source, prepared, result);
    }

    private static bool IntersectsTileInterior(Geometry source, Geometry tileGeometry)
    {
        return source.Relate(tileGeometry).Matches("T********");
    }

    private static bool LineBoundaryBelongsToTile(Geometry source, Geometry tileGeometry, TileId tile)
    {
        if (!(source is LineString))
        {
            return false;
        }

        var lastIndex = TileMath.TilesPerAxis(tile.Z) - 1;
        return LineIntersectionBelongsToTile(source.Intersection(tileGeometry), tileGeometry.EnvelopeInternal, tile.X == lastIndex, tile.Y == lastIndex);
    }

    private static bool LineIntersectionBelongsToTile(Geometry intersection, Envelope bounds, bool ownsEastEdge, bool ownsSouthEdge)
    {
        if (intersection.IsEmpty)
        {
            return false;
        }

        if (intersection is GeometryCollection collection)
        {
            for (var index = 0; index < collection.NumGeometries; index++)
            {
                if (LineIntersectionBelongsToTile(collection.GetGeometryN(index), bounds, ownsEastEdge, ownsSouthEdge))
                {
                    return true;
                }
            }

            return false;
        }

        if (!(intersection is LineString line))
        {
            return false;
        }

        // InteriorPoint may return an endpoint. Classify positive-length boundary segments
        // directly so ownership is independent of line direction and projection rounding.
        var sequence = line.CoordinateSequence;
        for (var index = 1; index < sequence.Count; index++)
        {
            var x0 = sequence.GetX(index - 1);
            var y0 = sequence.GetY(index - 1);
            var x1 = sequence.GetX(index);
            var y1 = sequence.GetY(index);

            // West/north edges belong to this tile; the outer world edges have no neighbor.
            if (x0 == x1 && y0 != y1 && (x0 == bounds.MinX || (ownsEastEdge && x0 == bounds.MaxX)))
            {
                return true;
            }

            if (y0 == y1 && x0 != x1 && (y0 == bounds.MaxY || (ownsSouthEdge && y0 == bounds.MinY)))
            {
                return true;
            }
        }

        return false;
    }
}
