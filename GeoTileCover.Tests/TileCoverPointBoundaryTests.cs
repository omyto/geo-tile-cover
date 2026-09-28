using System;
using System.Linq;
using NetTopologySuite.Geometries;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverPointBoundaryTests
{
    private static readonly GeometryFactory Factory = new GeometryFactory(new PrecisionModel(), srid: 4326);

    [Fact]
    public void Point_on_reported_horizontal_boundary_belongs_to_southern_row()
    {
        AssertTile(45, 66.51326044311186, new TileId(2, 2, 1));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void Every_low_zoom_row_boundary_distinguishes_points_on_and_immediately_beside_it(int zoom)
    {
        for (var row = 1; row < (1 << zoom); row++)
        {
            AssertRowBoundary(zoom, row);
        }
    }

    [Theory]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(25)]
    public void High_zoom_row_boundaries_distinguish_points_on_and_immediately_beside_them(int zoom)
    {
        var size = 1 << zoom;
        foreach (var row in new[] { 1, 2, 3, size / 4, size / 2 - 1, size / 2, size / 2 + 1, size * 3 / 4, size - 3, size - 2, size - 1 })
        {
            AssertRowBoundary(zoom, row);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(24)]
    [InlineData(25)]
    public void Points_on_and_just_inside_world_latitude_limits_stay_in_outer_rows(int zoom)
    {
        var size = 1 << zoom;
        var northernTile = new TileId(zoom, size / 2, 0);
        var southernTile = new TileId(zoom, size / 2, size - 1);
        var north = northernTile.ToEnvelope().MaxY;
        var south = southernTile.ToEnvelope().MinY;
        AssertTile(0, north, northernTile);
        AssertTile(0, Math.BitDecrement(north), northernTile);
        AssertTile(0, south, southernTile);
        AssertTile(0, Math.BitIncrement(south), southernTile);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(4, 3)]
    [InlineData(4, 13)]
    public void Boundary_points_have_consistent_parents_regardless_of_request_order(int boundaryZoom, int boundaryRow)
    {
        var latitude = new TileId(boundaryZoom, 0, boundaryRow).ToEnvelope().MaxY;
        foreach (var value in new[] { Math.BitIncrement(latitude), latitude, Math.BitDecrement(latitude) })
        {
            var point = Factory.CreatePoint(new Coordinate(0, value));
            var finestRow = (boundaryRow << (TileCover.MaxZoom - boundaryZoom)) - (value > latitude ? 1 : 0);
            var finest = new TileId(TileCover.MaxZoom, 1 << (TileCover.MaxZoom - 1), finestRow);
            var descending = new TileCover(point);
            var range = new TileCover(point).GetTiles(0, TileCover.MaxZoom).ToArray();
            var ascending = new TileCover(point);
            for (var zoom = TileCover.MaxZoom; zoom >= 0; zoom--)
            {
                var shift = TileCover.MaxZoom - zoom;
                var expected = new TileId(zoom, finest.X >> shift, finest.Y >> shift);
                Assert.Equal(expected, Assert.Single(descending.GetTiles(zoom)));
            }

            for (var zoom = 0; zoom <= TileCover.MaxZoom; zoom++)
            {
                var shift = TileCover.MaxZoom - zoom;
                var expected = new TileId(zoom, finest.X >> shift, finest.Y >> shift);
                AssertTile(0, value, expected);
                Assert.Equal(expected, Assert.Single(ascending.GetTiles(zoom)));
                Assert.Equal(expected, Assert.Single(descending.GetTiles(zoom)));
                Assert.Equal(expected, range[zoom]);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(25)]
    public void Points_inside_sampled_tiles_keep_their_tile(int zoom)
    {
        var random = new Random(173);
        for (var index = 0; index < 100; index++)
        {
            var tile = new TileId(zoom, random.Next(1 << zoom), random.Next(1 << zoom));
            var bounds = tile.ToEnvelope();
            AssertTile((bounds.MinX + bounds.MaxX) / 2, (bounds.MinY + bounds.MaxY) / 2, tile);
        }
    }

    private static void AssertRowBoundary(int zoom, int row)
    {
        var tile = new TileId(zoom, (1 << zoom) / 2, row);
        var bounds = tile.ToEnvelope();
        var longitude = (bounds.MinX + bounds.MaxX) / 2;
        AssertTile(longitude, bounds.MaxY, tile);
        AssertTile(longitude, Math.BitDecrement(bounds.MaxY), tile);
        AssertTile(longitude, Math.BitIncrement(bounds.MaxY), new TileId(zoom, tile.X, row - 1));
    }

    private static void AssertTile(double longitude, double latitude, TileId expected)
    {
        var point = Factory.CreatePoint(new Coordinate(longitude, latitude));
        Assert.Equal(expected, Assert.Single(new TileCover(point).GetTiles(expected.Z)));
    }
}
