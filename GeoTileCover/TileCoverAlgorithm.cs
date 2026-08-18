using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace GeoTileCover;

internal static class TileCoverAlgorithm
{
    public static IEnumerable<TileId> GetTiles(Geometry geometry, int minZoom, int maxZoom)
    {
        var maxZoomTiles = CoverAtZoom(geometry, maxZoom);
        var levels = new List<HashSet<TileId>>(maxZoom - minZoom + 1) { maxZoomTiles };
        var current = maxZoomTiles;

        for (var zoom = maxZoom - 1; zoom >= minZoom; zoom--)
        {
            var parents = new HashSet<TileId>();
            foreach (var tile in current)
            {
                parents.Add(tile.Parent());
            }

            levels.Add(parents);
            current = parents;
        }

        for (var level = levels.Count - 1; level >= 0; level--)
        {
            var ordered = new List<TileId>(levels[level]);
            ordered.Sort(TileIdComparer.Instance);
            foreach (var tile in ordered)
            {
                yield return tile;
            }
        }
    }

    private static HashSet<TileId> CoverAtZoom(Geometry geometry, int zoom)
    {
        var result = new HashSet<TileId>();
        CollectAtZoom(geometry, zoom, result);
        return result;
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

    private sealed class TileIdComparer : IComparer<TileId>
    {
        public static readonly TileIdComparer Instance = new TileIdComparer();

        public int Compare(TileId left, TileId right)
        {
            var byY = left.Y.CompareTo(right.Y);
            return byY != 0 ? byY : left.X.CompareTo(right.X);
        }
    }
}
