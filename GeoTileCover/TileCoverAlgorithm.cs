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

        return LineIntersectionBelongsToTile(source.Intersection(tileGeometry), tile);
    }

    private static bool LineIntersectionBelongsToTile(Geometry intersection, TileId tile)
    {
        if (intersection.IsEmpty)
        {
            return false;
        }

        if (intersection is GeometryCollection collection)
        {
            for (var index = 0; index < collection.NumGeometries; index++)
            {
                if (LineIntersectionBelongsToTile(collection.GetGeometryN(index), tile))
                {
                    return true;
                }
            }

            return false;
        }

        if (intersection.Dimension != Dimension.Curve)
        {
            return false;
        }

        var coordinate = intersection.InteriorPoint.Coordinate;
        return coordinate != null && TileMath.ToTile(coordinate.X, coordinate.Y, tile.Z) == tile;
    }
}
