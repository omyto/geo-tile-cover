using System;
using System.Linq;
using NetTopologySuite.Geometries;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverValidationTests
{
    [Fact]
    public void Rejects_null_geometry()
    {
        Assert.Throws<ArgumentNullException>(() => new TileCover(null!));
    }

    [Theory]
    [InlineData(-1, 1, true)]
    [InlineData(26, 26, true)]
    [InlineData(-1, 1, false)]
    [InlineData(0, 26, false)]
    public void Rejects_zoom_outside_supported_range(int minZoom, int maxZoom, bool useSingleZoomOverload)
    {
        var point = new GeometryFactory().CreatePoint(new Coordinate(10, 10));
        var cover = new TileCover(point);

        if (useSingleZoomOverload)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => cover.GetTiles(minZoom));
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => cover.GetTiles(minZoom, maxZoom));
        }
    }

    [Fact]
    public void Rejects_max_zoom_less_than_min_zoom()
    {
        var point = new GeometryFactory().CreatePoint(new Coordinate(10, 10));
        var cover = new TileCover(point);
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => cover.GetTiles(minZoom: 5, maxZoom: 4));

        Assert.Equal("maxZoom", exception.ParamName);
    }

    [Fact]
    public void Exposes_supported_zoom_range()
    {
        Assert.Equal(0, TileCover.MinZoom);
        Assert.Equal(25, TileCover.MaxZoom);
    }

    [Fact]
    public void Accepts_unspecified_and_wgs84_srid()
    {
        var unspecified = new GeometryFactory(new PrecisionModel(), srid: 0).CreatePoint(new Coordinate(10, 10));
        var wgs84 = new GeometryFactory(new PrecisionModel(), srid: 4326).CreatePoint(new Coordinate(10, 10));

        Assert.NotEmpty(new TileCover(unspecified).GetTiles(1));
        Assert.NotEmpty(new TileCover(wgs84).GetTiles(1));
    }

    [Fact]
    public void Rejects_non_wgs84_srid()
    {
        var geometry = new GeometryFactory(new PrecisionModel(), srid: 3857).CreatePoint(new Coordinate(0, 0));
        var exception = Assert.Throws<ArgumentException>(() => new TileCover(geometry));

        Assert.Equal("geometry", exception.ParamName);
    }

    [Fact]
    public void Empty_geometry_returns_no_tiles()
    {
        var empty = new GeometryFactory().CreatePolygon();
        Assert.Empty(new TileCover(empty).GetTiles(0, 3));
    }

    [Theory]
    [InlineData(-180.000001, 0)]
    [InlineData(180.000001, 0)]
    [InlineData(0, -85.051129)]
    [InlineData(0, 85.051129)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(0, double.NegativeInfinity)]
    public void Rejects_coordinates_outside_web_mercator_bounds(double longitude, double latitude)
    {
        var point = new GeometryFactory().CreatePoint(new Coordinate(longitude, latitude));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TileCover(point));
    }

    [Fact]
    public void Accepts_coordinates_on_web_mercator_bounds()
    {
        var factory = new GeometryFactory();
        var northWest = factory.CreatePoint(new Coordinate(-180, 85.0511287798066));
        var southEast = factory.CreatePoint(new Coordinate(180, -85.0511287798066));

        Assert.NotEmpty(new TileCover(northWest).GetTiles(1));
        Assert.NotEmpty(new TileCover(southEast).GetTiles(1));
    }

    [Fact]
    public void Rejects_line_crossing_the_antimeridian()
    {
        var line = new GeometryFactory().CreateLineString(new[]
        {
            new Coordinate(179, 10),
            new Coordinate(-179, 10)
        });

        Assert.Throws<NotSupportedException>(() => new TileCover(line));
    }

    [Fact]
    public void Rejects_polygon_crossing_the_antimeridian()
    {
        var polygon = new GeometryFactory().CreatePolygon(new[]
        {
            new Coordinate(179, 10),
            new Coordinate(-179, 10),
            new Coordinate(-179, -10),
            new Coordinate(179, -10),
            new Coordinate(179, 10)
        });

        Assert.Throws<NotSupportedException>(() => new TileCover(polygon));
    }
}
