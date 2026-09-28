using System;
using System.Linq;
using NetTopologySuite.Geometries;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverLineBoundaryTests
{
    private static readonly GeometryFactory Factory = new GeometryFactory(new PrecisionModel(), srid: 4326);

    [Theory]
    [InlineData(0, 20, 1, 0)]
    [InlineData(-20, 0, 0, 1)]
    public void Boundary_line_starting_at_a_corner_is_independent_of_direction_and_vertices(double endX, double endY, int x, int y)
    {
        AssertCoverage(new[] { new Coordinate(0, 0), new Coordinate(endX, endY) }, new TileId(1, x, y));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(25)]
    public void Full_tile_edges_belong_to_eastern_and_southern_tiles(int zoom)
    {
        var size = 1 << zoom;
        foreach (var row in new[] { 1, size / 4, size / 2, size - 1 }.Distinct())
        {
            var tile = new TileId(zoom, size / 2, row);
            var bounds = tile.ToEnvelope();
            AssertCoverage(new[] { new Coordinate(bounds.MinX, bounds.MinY), new Coordinate(bounds.MinX, bounds.MaxY) }, tile);
            AssertCoverage(new[] { new Coordinate(bounds.MinX, bounds.MaxY), new Coordinate(bounds.MaxX, bounds.MaxY) }, tile);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(25)]
    public void Lines_on_all_four_world_edges_are_retained(int zoom)
    {
        var size = 1 << zoom;
        foreach (var x in new[] { 0, size - 1 }.Distinct())
        {
            var tile = new TileId(zoom, x, size / 2);
            var bounds = tile.ToEnvelope();
            foreach (var longitude in new[] { bounds.MinX, bounds.MaxX }.Where(value => Math.Abs(value) == 180))
            {
                AssertCoverage(new[]
                {
                    new Coordinate(longitude, bounds.MinY + bounds.Height / 4),
                    new Coordinate(longitude, bounds.MinY + bounds.Height * 3 / 4)
                }, tile);
            }
        }

        foreach (var y in new[] { 0, size - 1 }.Distinct())
        {
            var tile = new TileId(zoom, size / 2, y);
            var bounds = tile.ToEnvelope();
            var latitudes = zoom == 0 ? new[] { bounds.MinY, bounds.MaxY } : new[] { y == 0 ? bounds.MaxY : bounds.MinY };
            foreach (var latitude in latitudes)
            {
                AssertCoverage(new[]
                {
                    new Coordinate(bounds.MinX + bounds.Width / 4, latitude),
                    new Coordinate(bounds.MinX + bounds.Width * 3 / 4, latitude)
                }, tile);
            }
        }
    }

    [Fact]
    public void Boundary_line_crossing_a_corner_covers_both_positive_length_parts()
    {
        AssertCoverage(new[] { new Coordinate(0, -20), new Coordinate(0, 20) }, new TileId(1, 1, 0), new TileId(1, 1, 1));
        AssertCoverage(new[] { new Coordinate(-20, 0), new Coordinate(20, 0) }, new TileId(1, 0, 1), new TileId(1, 1, 1));
    }

    [Fact]
    public void Bent_line_on_two_tile_edges_owns_each_positive_length_part()
    {
        AssertCoverage(new[] { new Coordinate(-20, 0), new Coordinate(0, 0), new Coordinate(0, 20) },
            new TileId(1, 1, 0), new TileId(1, 0, 1));
    }

    [Fact]
    public void Corner_touch_without_a_boundary_segment_does_not_add_neighbors()
    {
        AssertCoverage(new[] { new Coordinate(-20, 20), new Coordinate(0, 0) }, new TileId(1, 0, 0));
    }

    [Fact]
    public void Line_intersection_with_disconnected_boundary_parts_keeps_owned_parts()
    {
        AssertCoverage(new[]
        {
            new Coordinate(0, 10), new Coordinate(0, 20), new Coordinate(-10, 20),
            new Coordinate(-10, 40), new Coordinate(0, 40), new Coordinate(0, 50)
        }, new TileId(1, 0, 0), new TileId(1, 1, 0));
    }

    [Fact]
    public void Lines_immediately_beside_a_boundary_remain_on_their_own_side()
    {
        var bounds = new TileId(2, 3, 1).ToEnvelope();
        foreach (var east in new[] { false, true })
        {
            var longitude = east ? Math.BitIncrement(bounds.MinX) : Math.BitDecrement(bounds.MinX);
            AssertCoverage(new[] { new Coordinate(longitude, 10), new Coordinate(longitude, 20) }, new TileId(2, east ? 3 : 2, 1));
        }

        foreach (var south in new[] { false, true })
        {
            var latitude = south ? Math.BitDecrement(bounds.MaxY) : Math.BitIncrement(bounds.MaxY);
            AssertCoverage(new[] { new Coordinate(100, latitude), new Coordinate(110, latitude) }, new TileId(2, 3, south ? 1 : 0));
        }
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-20, 0)]
    public void Boundary_line_results_do_not_depend_on_zoom_request_order(double endX, double endY)
    {
        var line = Factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(endX, endY) });
        var ascending = new TileCover(line);
        var descending = new TileCover(line);
        var range = new TileCover(line).GetTiles(0, 4).ToArray();
        for (var zoom = 4; zoom >= 0; zoom--)
        {
            Assert.Equal(new TileCover(line).GetTiles(zoom), descending.GetTiles(zoom));
        }

        for (var zoom = 0; zoom <= 4; zoom++)
        {
            var expected = new TileCover(line).GetTiles(zoom).ToArray();
            Assert.NotEmpty(expected);
            Assert.Equal(expected, ascending.GetTiles(zoom));
            Assert.Equal(expected, descending.GetTiles(zoom));
            Assert.Equal(expected, range.Where(tile => tile.Z == zoom));
        }
    }

    private static void AssertCoverage(Coordinate[] coordinates, params TileId[] expected)
    {
        var subdivided = coordinates.Take(coordinates.Length - 1).SelectMany((point, index) => new[]
        {
            point, point, new Coordinate((point.X + coordinates[index + 1].X) / 2, (point.Y + coordinates[index + 1].Y) / 2)
        }).Concat(new[] { coordinates.Last() }).ToArray();
        foreach (var vertices in new[] { coordinates, coordinates.Reverse().ToArray(), subdivided, subdivided.Reverse().ToArray() })
        {
            var line = Factory.CreateLineString(vertices);
            Assert.Equal(expected, new TileCover(line).GetTiles(expected[0].Z));
        }
    }
}
