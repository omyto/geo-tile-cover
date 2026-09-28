using System;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverMinimalTests
{
    private static readonly GeometryFactory Factory = new GeometryFactory();

    [Theory]
    [InlineData("POINT (0 0)")]
    [InlineData("MULTIPOINT ((-10 10), (10 10), (-10 -10), (10 -10))")]
    [InlineData("LINESTRING (0 0, 0 20)")]
    [InlineData("LINESTRING (-180 -20, -180 20)")]
    [InlineData("LINESTRING (180 -20, 180 20)")]
    [InlineData("LINESTRING (-10 -85.0511287798066, 10 -85.0511287798066)")]
    [InlineData("LINESTRING (-10 85.0511287798066, 10 85.0511287798066)")]
    [InlineData("LINESTRING (-20 0, 0 0, 0 20)")]
    [InlineData("MULTILINESTRING ((-60 -20, 60 20), (0 -30, 0 30))")]
    [InlineData("POLYGON ((-70 -50, 60 -50, 60 60, -70 60, -70 -50), (-20 -20, -20 20, 20 20, 20 -20, -20 -20))")]
    [InlineData("MULTIPOLYGON (((-40 -20, 0 -20, 0 20, -40 20, -40 -20)), ((20 -20, 40 -20, 40 20, 20 20, 20 -20)))")]
    [InlineData("GEOMETRYCOLLECTION (POINT (120 30), LINESTRING (0 0, 0 20), POLYGON ((-20 -20, 20 -20, 20 20, -20 20, -20 -20)), POLYGON ((0 0, 40 0, 40 40, 0 40, 0 0)))")]
    [InlineData("POLYGON EMPTY")]
    [InlineData("GEOMETRYCOLLECTION EMPTY")]
    public void Minimal_cover_matches_full_cover_and_is_canonical(string wkt)
    {
        var geometry = ReadGeometry(wkt);
        for (var maxZoom = 0; maxZoom <= 4; maxZoom++)
        {
            var full = new TileCover(geometry).GetTiles(maxZoom).ToArray();
            var canonical = new TileCover(full);
            for (var minZoom = 0; minZoom <= maxZoom; minZoom++)
            {
                // Use a fresh geometry cover so a full-result cache cannot hide the new traversal.
                var actual = new TileCover(geometry).GetMinimalTiles(minZoom, maxZoom);
                Assert.Equal(canonical.GetMinimalTiles(minZoom, maxZoom), actual);
                Assert.Equal(full, new TileCover(actual).GetTiles(maxZoom));
                Assert.All(actual, tile => Assert.InRange(tile.Z, minZoom, maxZoom));
            }
        }
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(24, 0)]
    [InlineData(25, 0)]
    [InlineData(25, 3)]
    public void Full_tile_polygon_stops_before_expanding_to_max_zoom(int maxZoom, int minZoom)
    {
        var factory = new BudgetGeometryFactory();
        var tile = new TileId(1, 1, 0);
        var polygon = factory.ToGeometry(tile.ToEnvelope());
        var actual = new TileCover(polygon).GetMinimalTiles(minZoom, maxZoom);
        Assert.Equal(new TileCover(new[] { tile }).GetMinimalTiles(minZoom, maxZoom), actual);
    }

    [Fact]
    public void Components_compact_together_and_duplicate_coverage_is_skipped()
    {
        var factory = new BudgetGeometryFactory();
        var children = new[] { new TileId(2, 2, 0), new TileId(2, 3, 0), new TileId(2, 2, 1), new TileId(2, 3, 1) };
        var polygons = children.Select(tile => (Polygon)factory.ToGeometry(tile.ToEnvelope())).ToArray();
        var collection = factory.CreateGeometryCollection(new Geometry[]
        {
            factory.CreateMultiPolygon(polygons), factory.CreateMultiPolygon(polygons.Reverse().ToArray()),
            factory.CreatePoint(new Coordinate(10, 10)), factory.CreateLineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) })
        });
        Assert.Equal(new[] { new TileId(1, 1, 0) }, new TileCover(collection).GetMinimalTiles(0, 25));
    }

    [Fact]
    public void Four_points_compact_at_requested_resolution_without_filling_the_geometry()
    {
        var children = new[] { new TileId(2, 2, 0), new TileId(2, 3, 0), new TileId(2, 2, 1), new TileId(2, 3, 1) };
        var points = children.Select(tile =>
        {
            var bounds = tile.ToEnvelope();
            return new Coordinate((bounds.MinX + bounds.MaxX) / 2, (bounds.MinY + bounds.MaxY) / 2);
        }).ToArray();
        var geometry = Factory.CreateMultiPointFromCoords(points);
        var cover = new TileCover(geometry);
        Assert.Equal(new[] { new TileId(1, 1, 0) }, cover.GetMinimalTiles(0, 2));
        var expected = new TileCover(geometry).GetTiles(3).ToArray();
        Assert.Equal(4, expected.Length);
        Assert.Equal(expected, cover.GetTiles(3));
        Assert.Equal(new TileCover(expected).GetMinimalTiles(0, 3), cover.GetMinimalTiles(0, 3));
    }

    [Theory]
    [InlineData(24)]
    [InlineData(25)]
    public void Small_high_zoom_polygons_and_boundary_lines_preserve_ownership(int zoom)
    {
        var tile = new TileId(zoom, (1 << zoom) / 2, (1 << zoom) / 4);
        var bounds = tile.ToEnvelope();
        var geometries = new Geometry[]
        {
            Factory.ToGeometry(bounds),
            Factory.CreateLineString(new[] { new Coordinate(bounds.MinX, bounds.MinY), new Coordinate(bounds.MinX, bounds.MaxY) }),
            Factory.CreateLineString(new[] { new Coordinate(bounds.MinX, bounds.MaxY), new Coordinate(bounds.MaxX, bounds.MaxY) })
        };
        foreach (var geometry in geometries)
        {
            Assert.Equal(new[] { tile }, new TileCover(geometry).GetMinimalTiles(0, zoom));
            Assert.Equal(new[] { tile }, new TileCover(geometry.Reverse()).GetMinimalTiles(0, zoom));
        }
    }

    [Fact]
    public void Minimal_cache_is_scoped_to_both_zooms_and_returns_independent_arrays()
    {
        var tile = new TileId(1, 1, 0);
        var cover = new TileCover(new BudgetGeometryFactory().ToGeometry(tile.ToEnvelope()));
        var first = cover.GetMinimalTiles(0, 25);
        Assert.Equal(new[] { tile }, first);
        first[0] = new TileId(0, 0, 0);
        Assert.Equal(new[] { tile }, cover.GetMinimalTiles(0, 25));
        Assert.Equal(new TileCover(new[] { tile }).GetTiles(3), cover.GetMinimalTiles(3, 25));
        Assert.Equal(new[] { tile }, cover.GetMinimalTiles(0, 1));
        Assert.True(cover.TryGetTiles(2, 4, out var tiles, TestContext.Current.CancellationToken));
        Assert.Equal(new TileCover(new[] { tile }).GetTiles(2), tiles);
    }

    [Fact]
    public void Existing_full_tile_cache_gives_the_same_minimal_cover()
    {
        var geometry = ReadGeometry("POLYGON ((10 10, 100 10, 100 70, 10 70, 10 10))");
        var cold = new TileCover(geometry);
        var warm = new TileCover(geometry);
        var full = warm.GetTiles(5).ToArray();
        Assert.Equal(cold.GetMinimalTiles(1, 4), warm.GetMinimalTiles(1, 4));
        Assert.Equal(cold.GetMinimalTiles(2, 5), warm.GetMinimalTiles(2, 5));
        Assert.Equal(full, warm.GetTiles(5));
    }

    private static Geometry ReadGeometry(string wkt)
    {
        var geometry = new WKTReader().Read(wkt);
        geometry.SRID = 4326;
        return geometry;
    }

    private sealed class BudgetGeometryFactory : GeometryFactory
    {
        private int _tileGeometryCount;

        public override Geometry ToGeometry(Envelope envelope)
        {
            // Fail promptly if a small compact cover regresses to exhaustive traversal,
            // instead of trying to enumerate trillions of leaves in the high-zoom tests.
            if (++_tileGeometryCount > 256)
            {
                throw new InvalidOperationException("Compact cover exceeded its tile geometry budget.");
            }

            return base.ToGeometry(envelope);
        }
    }
}
