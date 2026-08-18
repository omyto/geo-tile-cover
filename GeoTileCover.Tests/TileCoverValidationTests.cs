using System;
using System.Linq;
using NetTopologySuite.Geometries;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverValidationTests
{
    [Fact]
    public void Rejects_null_geometry_when_results_are_enumerated()
    {
        Assert.Throws<ArgumentNullException>(() => TileCover.GetTiles(null!, 1).ToArray());
    }

    [Theory]
    [InlineData(-1, 1, true)]
    [InlineData(26, 26, true)]
    [InlineData(-1, 1, false)]
    [InlineData(0, 26, false)]
    public void Rejects_zoom_outside_supported_range(int minZoom, int maxZoom, bool useSingleZoomOverload)
    {
        var point = new GeometryFactory().CreatePoint(new Coordinate(10, 10));

        if (useSingleZoomOverload)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TileCover.GetTiles(point, minZoom).ToArray());
        }
        else
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TileCover.GetTiles(point, minZoom, maxZoom).ToArray());
        }
    }

    [Fact]
    public void Rejects_max_zoom_less_than_min_zoom()
    {
        var point = new GeometryFactory().CreatePoint(new Coordinate(10, 10));
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => TileCover.GetTiles(point, minZoom: 5, maxZoom: 4).ToArray());

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

        Assert.NotEmpty(TileCover.GetTiles(unspecified, 1));
        Assert.NotEmpty(TileCover.GetTiles(wgs84, 1));
    }

    [Fact]
    public void Rejects_non_wgs84_srid()
    {
        var geometry = new GeometryFactory(new PrecisionModel(), srid: 3857).CreatePoint(new Coordinate(0, 0));
        var exception = Assert.Throws<ArgumentException>(() => TileCover.GetTiles(geometry, 1).ToArray());

        Assert.Equal("geometry", exception.ParamName);
    }

    [Fact]
    public void Empty_geometry_returns_no_tiles()
    {
        var empty = new GeometryFactory().CreatePolygon();
        Assert.Empty(TileCover.GetTiles(empty, 0, 3));
    }
}
