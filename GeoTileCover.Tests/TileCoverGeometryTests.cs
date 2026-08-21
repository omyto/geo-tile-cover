using System.Linq;
using NetTopologySuite.Geometries;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverGeometryTests
{
    private static readonly GeometryFactory Factory = new GeometryFactory(new PrecisionModel(), srid: 4326);

    [Fact]
    public void Point_returns_one_tile_at_each_zoom()
    {
        var point = Factory.CreatePoint(new Coordinate(105.8342, 21.0278));

        Assert.Equal(new[]
        {
            new TileId(0, 0, 0),
            new TileId(1, 1, 0),
            new TileId(2, 3, 1)
        }, Cover(point).GetTiles(0, 2));
    }

    [Fact]
    public void Single_zoom_overload_returns_only_that_zoom()
    {
        var point = Factory.CreatePoint(new Coordinate(105.8342, 21.0278));
        Assert.Equal(new[] { new TileId(10, 813, 450) }, Cover(point).GetTiles(10));
    }

    [Fact]
    public void Line_crossing_the_prime_meridian_intersects_both_tiles()
    {
        var line = Factory.CreateLineString(new[]
        {
            new Coordinate(-10, 20),
            new Coordinate(10, 20)
        });

        Assert.Equal(new[]
        {
            new TileId(1, 0, 0),
            new TileId(1, 1, 0)
        }, Cover(line).GetTiles(1));
    }

    [Fact]
    public void Polygon_contained_by_one_tile_returns_that_tile()
    {
        var polygon = Rectangle(10, 10, 20, 20);
        Assert.Equal(new[] { new TileId(1, 1, 0) }, Cover(polygon).GetTiles(1));
    }

    [Fact]
    public void MultiPolygon_returns_tiles_for_disconnected_parts()
    {
        var multiPolygon = Factory.CreateMultiPolygon(new[]
        {
            Rectangle(-20, 10, -10, 20),
            Rectangle(10, -20, 20, -10)
        });

        Assert.Equal(new[]
        {
            new TileId(1, 0, 0),
            new TileId(1, 1, 1)
        }, Cover(multiPolygon).GetTiles(1));
    }

    [Fact]
    public void Range_derives_and_deduplicates_parent_tiles()
    {
        var polygon = Rectangle(10, 10, 100, 70);

        Assert.Equal(new[]
        {
            new TileId(1, 1, 0),
            new TileId(2, 2, 0),
            new TileId(2, 3, 0),
            new TileId(2, 2, 1),
            new TileId(2, 3, 1)
        }, Cover(polygon).GetTiles(1, 2));
    }

    [Fact]
    public void Results_are_ordered_by_zoom_then_y_then_x()
    {
        var geometry = Factory.CreateMultiPointFromCoords(new[]
        {
            new Coordinate(100, -20),
            new Coordinate(-100, 20),
            new Coordinate(20, 20)
        });

        var tiles = Cover(geometry).GetTiles(1, 2).ToArray();

        Assert.Equal(tiles.OrderBy(tile => tile.Z).ThenBy(tile => tile.Y).ThenBy(tile => tile.X), tiles);
    }

    [Fact]
    public void Point_on_tile_corner_belongs_to_one_xyz_tile()
    {
        var point = Factory.CreatePoint(new Coordinate(0, 0));

        Assert.Equal(new[] { new TileId(1, 1, 1) }, Cover(point).GetTiles(1));
    }

    [Fact]
    public void Point_on_tile_edge_belongs_to_one_xyz_tile()
    {
        var point = Factory.CreatePoint(new Coordinate(0, 20));
        Assert.Equal(new[] { new TileId(1, 1, 0) }, Cover(point).GetTiles(1));
    }

    [Fact]
    public void Polygon_boundary_touch_does_not_include_neighboring_tiles()
    {
        var polygon = Rectangle(-10, -10, 0, 0);
        Assert.Equal(new[] { new TileId(1, 0, 1) }, Cover(polygon).GetTiles(1));
    }

    [Fact]
    public void Vertical_line_on_tile_boundary_belongs_to_eastern_tile()
    {
        var line = Factory.CreateLineString(new[]
        {
            new Coordinate(0, 10),
            new Coordinate(0, 20)
        });

        Assert.Equal(new[] { new TileId(1, 1, 0) }, Cover(line).GetTiles(1));
    }

    [Fact]
    public void Horizontal_line_on_tile_boundary_belongs_to_southern_tile()
    {
        var line = Factory.CreateLineString(new[]
        {
            new Coordinate(10, 0),
            new Coordinate(20, 0)
        });

        Assert.Equal(new[] { new TileId(1, 1, 1) }, Cover(line).GetTiles(1));
    }

    [Fact]
    public void Line_endpoint_touch_does_not_include_neighboring_tile()
    {
        var line = Factory.CreateLineString(new[]
        {
            new Coordinate(-10, 20),
            new Coordinate(0, 20)
        });

        Assert.Equal(new[] { new TileId(1, 0, 0) }, Cover(line).GetTiles(1));
    }

    [Fact]
    public void Point_supports_zoom_24_and_25()
    {
        var point = Factory.CreatePoint(new Coordinate(0, 0));

        Assert.Equal(new[]
        {
            new TileId(24, 8388608, 8388608),
            new TileId(25, 16777216, 16777216)
        }, Cover(point).GetTiles(24, 25));
    }

    [Fact]
    public void Cover_uses_a_snapshot_of_the_source_geometry()
    {
        var point = Factory.CreatePoint(new Coordinate(10, 10));
        var cover = new TileCover(point);

        point.CoordinateSequence.SetOrdinate(0, Ordinate.X, -100);
        point.CoordinateSequence.SetOrdinate(0, Ordinate.Y, -20);
        point.GeometryChanged();

        Assert.Equal(new[] { new TileId(1, 1, 0) }, cover.GetTiles(1));
    }

    [Fact]
    public void Repeated_and_overlapping_requests_return_consistent_tiles()
    {
        var cover = Cover(Rectangle(10, 10, 100, 70));
        var range = cover.GetTiles(1, 2).ToArray();

        Assert.Equal(range.Where(tile => tile.Z == 2), cover.GetTiles(2));
        Assert.Equal(range.Where(tile => tile.Z == 1), cover.GetTiles(1));
        Assert.Equal(range, cover.GetTiles(1, 2));
    }

    private static TileCover Cover(Geometry geometry) => new TileCover(geometry);

    private static Polygon Rectangle(double minX, double minY, double maxX, double maxY) =>
        Factory.CreatePolygon(new[]
        {
            new Coordinate(minX, minY),
            new Coordinate(maxX, minY),
            new Coordinate(maxX, maxY),
            new Coordinate(minX, maxY),
            new Coordinate(minX, minY)
        });
}
