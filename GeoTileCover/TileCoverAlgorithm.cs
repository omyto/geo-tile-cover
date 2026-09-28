using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace GeoTileCover;

internal static class TileCoverAlgorithm
{
    public static TileId[] GetTilesAtZoom(Geometry geometry, int zoom)
    {
        var result = new HashSet<TileId>();
        CollectAtZoom(geometry, zoom, result);
        return Sort(result);
    }

    public static TileId[] GetParentTiles(IEnumerable<TileId> tiles)
    {
        var parents = new HashSet<TileId>();
        foreach (var tile in tiles)
        {
            parents.Add(tile.Parent());
        }

        return Sort(parents);
    }

    private static TileId[] Sort(HashSet<TileId> tiles)
    {
        var ordered = new List<TileId>(tiles);
        ordered.Sort(TileIdComparer.Instance);
        return ordered.ToArray();
    }

    private static void CollectAtZoom(Geometry geometry, int zoom, HashSet<TileId> result)
    {
        if (geometry is GeometryCollection collection)
        {
            for (var index = 0; index < collection.NumGeometries; index++)
            {
                CollectAtZoom(collection.GetGeometryN(index), zoom, result);
            }

            return;
        }

        if (geometry.Dimension == Dimension.Point)
        {
            foreach (var coordinate in geometry.Coordinates)
            {
                result.Add(TileMath.ToTile(coordinate.X, coordinate.Y, zoom));
            }

            return;
        }

        var prepared = PreparedGeometryFactory.Prepare(geometry);
        Visit(new TileId(0, 0, 0), zoom, geometry, prepared, result);
    }

    private static void Visit(TileId tile, int targetZoom, Geometry source, IPreparedGeometry prepared, HashSet<TileId> result)
    {
        var tileGeometry = source.Factory.ToGeometry(TileMath.ToEnvelope(tile));
        if (!prepared.Intersects(tileGeometry))
        {
            return;
        }

        if (tile.Z == targetZoom)
        {
            if (IntersectsTileInterior(source, tileGeometry) || LineBoundaryBelongsToTile(source, tileGeometry, tile))
            {
                result.Add(tile);
            }

            return;
        }

        var z = tile.Z + 1;
        var x = tile.X << 1;
        var y = tile.Y << 1;
        Visit(new TileId(z, x, y), targetZoom, source, prepared, result);
        Visit(new TileId(z, x + 1, y), targetZoom, source, prepared, result);
        Visit(new TileId(z, x, y + 1), targetZoom, source, prepared, result);
        Visit(new TileId(z, x + 1, y + 1), targetZoom, source, prepared, result);
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
