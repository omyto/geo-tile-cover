using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileCoverTileTests
{
    [Fact]
    public void Full_tile_expands_to_children_and_rolls_up_to_parent()
    {
        var cover = new TileCover(new[] { new TileId(1, 0, 0) });

        Assert.Equal(new[] { new TileId(0, 0, 0) }, cover.GetTiles(0));
        Assert.Equal(new[]
        {
            new TileId(2, 0, 0),
            new TileId(2, 1, 0),
            new TileId(2, 0, 1),
            new TileId(2, 1, 1)
        }, cover.GetTiles(2));
    }

    [Fact]
    public void Mixed_zoom_tiles_form_a_full_tile_union()
    {
        var cover = new TileCover(new[]
        {
            new TileId(1, 0, 0),
            new TileId(2, 2, 0)
        });

        Assert.Equal(new[]
        {
            new TileId(1, 0, 0),
            new TileId(1, 1, 0)
        }, cover.GetTiles(1));
        Assert.Equal(new[]
        {
            new TileId(2, 0, 0),
            new TileId(2, 1, 0),
            new TileId(2, 2, 0),
            new TileId(2, 0, 1),
            new TileId(2, 1, 1)
        }, cover.GetTiles(2));
        Assert.Equal(20, cover.GetTiles(3).Count());
    }

    [Fact]
    public void Ancestor_makes_source_descendants_redundant()
    {
        var cover = new TileCover(new[]
        {
            new TileId(1, 0, 0),
            new TileId(2, 0, 0),
            new TileId(3, 1, 1)
        });

        Assert.Equal(16, cover.GetTiles(3).Count());
    }

    [Fact]
    public void Complete_sibling_groups_preserve_their_parent_union()
    {
        var cover = new TileCover(new[]
        {
            new TileId(2, 2, 2),
            new TileId(2, 3, 2),
            new TileId(2, 2, 3),
            new TileId(2, 3, 3)
        });

        Assert.Equal(new[] { new TileId(1, 1, 1) }, cover.GetTiles(1));
        Assert.Equal(16, cover.GetTiles(3).Count());
    }

    [Fact]
    public void Tile_constructor_takes_a_snapshot_and_deduplicates()
    {
        var source = new List<TileId>
        {
            new TileId(2, 2, 1),
            new TileId(2, 2, 1)
        };
        var cover = new TileCover(source);

        source.Clear();
        source.Add(new TileId(2, 0, 0));

        Assert.Equal(new[] { new TileId(2, 2, 1) }, cover.GetTiles(2));
    }

    [Fact]
    public void Empty_tile_union_returns_no_tiles_at_every_zoom()
    {
        var cover = new TileCover(new TileId[0]);

        Assert.Empty(cover.GetTiles(0, TileCover.MaxZoom));
    }

    [Fact]
    public void Repeated_mixed_zoom_requests_are_consistent()
    {
        var cover = new TileCover(new[]
        {
            new TileId(1, 0, 0),
            new TileId(2, 2, 0)
        });
        var range = cover.GetTiles(1, 3).ToArray();

        Assert.Equal(range.Where(tile => tile.Z == 2), cover.GetTiles(2));
        Assert.Equal(range.Where(tile => tile.Z == 1), cover.GetTiles(1));
        Assert.Equal(range, cover.GetTiles(1, 3));
    }
}
