using System;
using System.Collections.Generic;
using Xunit;

namespace GeoTileCover.Tests;

public sealed class TileIdTests
{
    [Fact]
    public void Constructor_exposes_xyz_coordinates()
    {
        var tile = new TileId(14, 12345, 6789);

        Assert.Equal(14, tile.Z);
        Assert.Equal(12345, tile.X);
        Assert.Equal(6789, tile.Y);
        Assert.Equal("14/12345/6789", tile.ToString());
    }

    [Fact]
    public void Id_uses_fixed_zxy_bit_layout()
    {
        var tile = new TileId(1, 1, 1);
        var expected = (1L << 50) | (1L << 25) | 1L;

        Assert.Equal(expected, tile.Id);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(14, 12345, 6789)]
    [InlineData(25, 33554431, 33554431)]
    public void Packed_id_round_trips_xyz(int z, int x, int y)
    {
        var expected = new TileId(z, x, y);
        var actual = new TileId(expected.Id);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(26L << 50)]
    [InlineData((1L << 50) | (2L << 25))]
    [InlineData((1L << 50) | 2L)]
    public void Id_constructor_rejects_invalid_packed_value(long id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TileId(id));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(26, 0, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 2, 0)]
    [InlineData(1, 0, -1)]
    [InlineData(1, 0, 2)]
    public void Constructor_rejects_coordinates_outside_the_zoom_grid(int z, int x, int y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TileId(z, x, y));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(14, 12345, 6789)]
    [InlineData(16, 65535, 65535)]
    [InlineData(16, 32768, 0)]
    public void Packed_xy_round_trips_coordinates(int z, int x, int y)
    {
        var expected = new TileId(z, x, y);
        var actual = new TileId(z, expected.PackedXY);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Packed_xy_uses_all_int_bits_at_zoom_16()
    {
        var tile = new TileId(16, 65535, 65535);

        Assert.Equal(-1, tile.PackedXY);
        Assert.Equal(tile, new TileId(16, -1));
    }

    [Fact]
    public void Try_get_packed_xy_returns_value_through_zoom_16()
    {
        var tile = new TileId(16, 65535, 65535);

        Assert.True(tile.TryGetPackedXY(out var packedXY));
        Assert.Equal(-1, packedXY);
    }

    [Fact]
    public void Packed_xy_is_unavailable_above_zoom_16()
    {
        var tile = new TileId(17, 0, 0);

        Assert.Throws<InvalidOperationException>(() => tile.PackedXY);
        Assert.False(tile.TryGetPackedXY(out var packedXY));
        Assert.Equal(default, packedXY);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(17, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 4)]
    public void Packed_xy_constructor_rejects_invalid_value(int z, int packedXY)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TileId(z, packedXY));
    }

    [Fact]
    public void Parent_uses_xyz_quadtree_coordinates()
    {
        Assert.Equal(new TileId(13, 6172, 3394), new TileId(14, 12345, 6789).Parent());
    }

    [Fact]
    public void Zoom_zero_tile_has_no_parent()
    {
        Assert.Throws<InvalidOperationException>(() => new TileId(0, 0, 0).Parent());
    }

    [Fact]
    public void Value_equality_supports_hash_based_deduplication()
    {
        var first = new TileId(8, 201, 114);
        var equal = new TileId(8, 201, 114);
        var different = new TileId(8, 201, 115);
        var set = new HashSet<TileId> { first, equal, different };

        Assert.True(first == equal);
        Assert.False(first != equal);
        Assert.NotEqual(first, different);
        Assert.Equal(2, set.Count);
    }
}
