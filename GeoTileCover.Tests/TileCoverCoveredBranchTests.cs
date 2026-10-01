using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverCoveredBranchTests
{
    private static readonly GeometryFactory Factory = new GeometryFactory();

    [Theory]
    [InlineData(7, false)]
    [InlineData(7, true)]
    [InlineData(24, false)]
    [InlineData(24, true)]
    [InlineData(25, false)]
    [InlineData(25, true)]
    public void Full_polygon_emits_descendants_without_testing_each_child_geometry(int zoom, bool bounded)
    {
        var sourceZoom = zoom - 6;
        var source = new TileId(sourceZoom, (1 << sourceZoom) / 2, (1 << sourceZoom) / 4);
        var factory = new BudgetGeometryFactory();
        var cover = new TileCover(factory.ToGeometry(source.ToEnvelope()));
        var expected = Descendants(source, zoom).ToArray();
        Assert.Equal(4_096, expected.Length);

        if (bounded)
        {
            Assert.True(cover.TryGetTiles(zoom, expected.Length, out var actual, TestContext.Current.CancellationToken));
            Assert.Equal(expected, actual);
        }
        else
        {
            Assert.Equal(expected, cover.GetTiles(zoom));
        }
    }

    [Theory]
    [InlineData(6)]
    [InlineData(24)]
    [InlineData(25)]
    public void Hole_inside_a_branch_excludes_its_descendants_and_boundary_only_neighbors(int zoom)
    {
        var sourceZoom = zoom - 4;
        var outer = new TileId(sourceZoom, (1 << sourceZoom) / 2, (1 << sourceZoom) / 4);
        var hole = new TileId(sourceZoom + 2, (outer.X << 2) + 1, (outer.Y << 2) + 1);
        var shell = ((Polygon)Factory.ToGeometry(outer.ToEnvelope())).Shell;
        var ring = ((Polygon)Factory.ToGeometry(hole.ToEnvelope())).Shell;
        var polygon = Factory.CreatePolygon(shell, new[] { ring });
        Assert.True(polygon.IsValid);

        var expected = Descendants(outer, zoom).Except(Descendants(hole, zoom)).ToArray();
        Assert.Equal(240, expected.Length);
        Assert.Equal(expected, new TileCover(polygon).GetTiles(zoom));
        Assert.True(new TileCover(polygon).TryGetTiles(zoom, expected.Length, out var actual, TestContext.Current.CancellationToken));
        Assert.Equal(expected, actual);
        Assert.False(new TileCover(polygon).TryGetTiles(zoom, expected.Length - 1, out var rejected, TestContext.Current.CancellationToken));
        Assert.Empty(rejected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Covered_branch_budget_counts_only_tiles_not_already_added_by_other_components(bool reverse)
    {
        var source = new TileId(2, 2, 1);
        var expected = Descendants(source, 5).ToArray();
        var points = expected.Select(tile =>
        {
            var bounds = tile.ToEnvelope();
            return new Coordinate((bounds.MinX + bounds.MaxX) / 2, (bounds.MinY + bounds.MaxY) / 2);
        }).ToArray();
        var polygon = Factory.ToGeometry(source.ToEnvelope());
        var components = new Geometry[] { Factory.CreateMultiPointFromCoords(points), polygon, polygon.Copy() };
        if (reverse)
        {
            Array.Reverse(components);
        }

        var cover = new TileCover(Factory.CreateGeometryCollection(components));
        Assert.True(cover.TryGetTiles(5, expected.Length, out var actual, TestContext.Current.CancellationToken));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("POLYGON ((-90 -60, 90 -60, 90 0, 0 0, 0 60, -90 60, -90 -60))")]
    [InlineData("POLYGON ((-90 0, 0 -60, 90 0, 0 60, -90 0))")]
    [InlineData("POLYGON ((-90 -1, 90 -1, 90 1, -90 1, -90 -1))")]
    [InlineData("MULTIPOLYGON (((-90 -60, 0 -60, 0 0, -90 0, -90 -60)), ((10 10, 90 10, 90 60, 10 60, 10 10)))")]
    public void Polygon_cover_matches_exhaustive_tile_interior_intersections(string wkt)
    {
        var geometry = new WKTReader().Read(wkt);
        geometry.SRID = 4326;
        Assert.True(geometry.IsValid);
        // Independent leaf-level oracle: no Covers check or quadtree pruning.
        var expected = Descendants(new TileId(0, 0, 0), 4)
            .Where(tile => geometry.Relate(Factory.ToGeometry(tile.ToEnvelope())).Matches("T********")).ToArray();
        Assert.Equal(expected, new TileCover(geometry).GetTiles(4));
        Assert.True(new TileCover(geometry).TryGetTiles(4, expected.Length, out var actual, TestContext.Current.CancellationToken));
        Assert.Equal(expected, actual);
    }

    private static IEnumerable<TileId> Descendants(TileId tile, int zoom)
    {
        var shift = zoom - tile.Z;
        for (var y = tile.Y << shift; y < ((tile.Y + 1) << shift); y++)
        {
            for (var x = tile.X << shift; x < ((tile.X + 1) << shift); x++)
            {
                yield return new TileId(zoom, x, y);
            }
        }
    }

    private sealed class BudgetGeometryFactory : GeometryFactory
    {
        private int _count;

        public override Geometry ToGeometry(Envelope envelope)
        {
            // Allow traversal of boundary-only neighbors, but not a geometry per
            // output tile inside the fully covered 4,096-tile branch.
            if (++_count > 3_000)
            {
                throw new InvalidOperationException("Covered branch performed too many geometry operations.");
            }

            return base.ToGeometry(envelope);
        }
    }
}
