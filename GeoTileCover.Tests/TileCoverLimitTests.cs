using System;
using System.Linq;
using System.Threading;
using NetTopologySuite.Geometries;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverLimitTests
{
    private static readonly GeometryFactory Factory = new GeometryFactory();

    [Theory]
    [InlineData(false, 15, false)]
    [InlineData(false, 16, true)]
    [InlineData(false, 17, true)]
    [InlineData(true, 15, false)]
    [InlineData(true, 16, true)]
    [InlineData(true, 17, true)]
    public void Limit_applies_to_the_complete_distinct_result(bool tileSource, int maxTiles, bool succeeds)
    {
        var cover = CreateCover(tileSource);
        Assert.Equal(succeeds, cover.TryGetTiles(3, maxTiles, out var tiles, TestContext.Current.CancellationToken));
        if (succeeds)
        {
            Assert.Equal(16, tiles.Length);
            Assert.Equal(CreateCover(tileSource).GetTiles(3), tiles);
        }
        else
        {
            Assert.Empty(tiles);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Exceeding_limit_does_not_cache_a_partial_result_or_limit_GetTiles(bool tileSource)
    {
        var cover = CreateCover(tileSource);
        Assert.False(cover.TryGetTiles(3, 3, out var rejected, TestContext.Current.CancellationToken));
        Assert.Empty(rejected);
        Assert.Equal(CreateCover(tileSource).GetTiles(3), cover.GetTiles(3));
        Assert.True(cover.TryGetTiles(3, 16, out var complete, TestContext.Current.CancellationToken));
        Assert.Equal(16, complete.Length);
        Assert.False(cover.TryGetTiles(3, 15, out rejected, TestContext.Current.CancellationToken));
        Assert.Empty(rejected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void High_zoom_cache_is_bounded_at_the_requested_zoom_only(bool tileSource)
    {
        var cover = CreateCover(tileSource);
        Assert.Equal(64, cover.GetTiles(4).Count());
        Assert.True(cover.TryGetTiles(1, 1, out var parent, TestContext.Current.CancellationToken));
        Assert.Equal(new[] { new TileId(1, 0, 0) }, parent);
        Assert.False(cover.TryGetTiles(3, 15, out var rejected, TestContext.Current.CancellationToken));
        Assert.Empty(rejected);
        Assert.True(cover.TryGetTiles(3, 16, out var tiles, TestContext.Current.CancellationToken));
        Assert.Equal(CreateCover(tileSource).GetTiles(3), tiles);
    }

    [Fact]
    public void Overlapping_geometry_components_count_each_tile_once()
    {
        var polygon = Factory.ToGeometry(new TileId(1, 0, 0).ToEnvelope());
        var collection = Factory.CreateGeometryCollection(new[] { polygon, polygon.Copy(), Factory.CreatePoint(new Coordinate(-10, 10)) });
        var cover = new TileCover(collection);
        Assert.True(cover.TryGetTiles(3, 16, out var tiles, TestContext.Current.CancellationToken));
        Assert.Equal(16, tiles.Length);
        Assert.Equal(new TileCover(polygon).GetTiles(3), tiles);
    }

    [Fact]
    public void Duplicate_points_and_mixed_source_tiles_do_not_consume_extra_budget()
    {
        var points = Factory.CreateMultiPointFromCoords(new[] { new Coordinate(10, 10), new Coordinate(10, 10), new Coordinate(20, 20) });
        Assert.True(new TileCover(points).TryGetTiles(1, 1, out var pointTiles, TestContext.Current.CancellationToken));
        Assert.Equal(new[] { new TileId(1, 1, 0) }, pointTiles);
        var cover = new TileCover(new[] { new TileId(3, 0, 0), new TileId(3, 0, 0), new TileId(4, 2, 0), new TileId(4, 3, 0) });
        Assert.True(cover.TryGetTiles(1, 1, out var tileTiles, TestContext.Current.CancellationToken));
        Assert.Equal(new[] { new TileId(1, 0, 0) }, tileTiles);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Zero_budget_accepts_only_empty_covers(bool tileSource)
    {
        var empty = tileSource ? new TileCover(Array.Empty<TileId>()) : new TileCover(Factory.CreateGeometryCollection());
        Assert.True(empty.TryGetTiles(25, 0, out var tiles, TestContext.Current.CancellationToken));
        Assert.Empty(tiles);
        Assert.True(empty.TryGetTiles(25, 0, out tiles, TestContext.Current.CancellationToken));
        Assert.False(CreateCover(tileSource).TryGetTiles(3, 0, out tiles, TestContext.Current.CancellationToken));
        Assert.Empty(tiles);
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(26, 1)]
    [InlineData(1, -1)]
    public void Invalid_arguments_are_rejected(int zoom, int maxTiles)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateCover(true).TryGetTiles(zoom, maxTiles, out _, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Returned_array_cannot_mutate_cached_results(bool warmCache)
    {
        var cover = CreateCover(true);
        if (warmCache)
        {
            _ = cover.GetTiles(3).ToArray();
        }

        Assert.True(cover.TryGetTiles(3, 16, out var tiles, TestContext.Current.CancellationToken));
        tiles[0] = new TileId(0, 0, 0);
        Assert.Equal(CreateCover(true).GetTiles(3), cover.GetTiles(3));
        Assert.True(cover.TryGetTiles(3, 16, out var again, TestContext.Current.CancellationToken));
        Assert.Equal(CreateCover(true).GetTiles(3), again);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Huge_covers_stop_at_the_limit_during_traversal(bool tileSource)
    {
        // At zoom 25 this source covers 4^24 tiles. Cancellation is a fail-safe if
        // traversal regresses to enumerating the complete cover before checking its size.
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var cover = CreateCover(tileSource);
        Assert.False(cover.TryGetTiles(25, 8, out var tiles, cancellation.Token));
        Assert.Empty(tiles);
        Assert.True(cover.TryGetTiles(1, 1, out var parent, TestContext.Current.CancellationToken));
        Assert.Equal(new[] { new TileId(1, 0, 0) }, parent);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cancellation_is_observed_even_for_cached_results(bool warmCache)
    {
        var cover = CreateCover(true);
        if (warmCache)
        {
            _ = cover.GetTiles(3).ToArray();
        }

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var tiles = new[] { new TileId(0, 0, 0) };
        Assert.Throws<OperationCanceledException>(() => cover.TryGetTiles(3, 16, out tiles, cancellation.Token));
        Assert.Empty(tiles);
        Assert.True(cover.TryGetTiles(3, 16, out tiles, TestContext.Current.CancellationToken));
        Assert.Equal(CreateCover(true).GetTiles(3), tiles);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Cancellation_during_large_traversal_releases_the_cover(bool tileSource)
    {
        var cover = CreateCover(tileSource);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
        var tiles = new[] { new TileId(0, 0, 0) };
        Assert.Throws<OperationCanceledException>(() => cover.TryGetTiles(25, int.MaxValue, out tiles, cancellation.Token));
        Assert.Empty(tiles);
        Assert.True(cover.TryGetTiles(3, 16, out tiles, TestContext.Current.CancellationToken));
        Assert.Equal(CreateCover(tileSource).GetTiles(3), tiles);
    }

    private static TileCover CreateCover(bool tileSource)
    {
        var tile = new TileId(1, 0, 0);
        return tileSource ? new TileCover(new[] { tile }) : new TileCover(Factory.ToGeometry(tile.ToEnvelope()));
    }
}
