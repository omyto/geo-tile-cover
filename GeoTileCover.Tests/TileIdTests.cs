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
        var tile = new TileId(TileId.PackedXYMaxZoom, 65535, 65535);

        Assert.Equal(-1, tile.PackedXY);
        Assert.Equal(tile, new TileId(TileId.PackedXYMaxZoom, -1));
    }

    [Fact]
    public void Try_get_packed_xy_returns_value_through_zoom_16()
    {
        var tile = new TileId(TileId.PackedXYMaxZoom, 65535, 65535);

        Assert.True(tile.TryGetPackedXY(out var packedXY));
        Assert.Equal(-1, packedXY);
    }

    [Fact]
    public void Packed_xy_is_unavailable_above_zoom_16()
    {
        var tile = new TileId(TileId.PackedXYMaxZoom + 1, 0, 0);

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

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(-12345)]
    public void Negative_packed_xy_is_rejected_below_zoom_16(int packedXY)
    {
        for (var zoom = 0; zoom < 16; zoom++)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TileId(zoom, packedXY));
            Assert.Equal("packedXY", exception.ParamName);
        }
    }

    [Theory]
    [InlineData(int.MinValue, 32768, 0)]
    [InlineData(int.MinValue + 1, 32768, 1)]
    [InlineData(-65536, 65535, 0)]
    [InlineData(-1, 65535, 65535)]
    public void Negative_packed_xy_round_trips_at_zoom_16(int packedXY, int x, int y)
    {
        var tile = new TileId(16, packedXY);
        Assert.Equal(new TileId(16, x, y), tile);
        Assert.Equal(packedXY, tile.PackedXY);
        Assert.Equal(tile, new TileId(tile.Id));
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
    public void To_envelope_returns_wgs84_geographic_bounds()
    {
        var envelope = new TileId(1, 1, 0).ToEnvelope();

        Assert.Equal(0d, envelope.MinX);
        Assert.Equal(180d, envelope.MaxX);
        Assert.Equal(0d, envelope.MinY, precision: 12);
        Assert.Equal(85.0511287798066d, envelope.MaxY, precision: 12);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(8, 201, 114)]
    [InlineData(16, 32768, 0)]
    [InlineData(16, 65535, 65535)]
    [InlineData(25, 33554431, 33554431)]
    public void Equal_tiles_from_supported_encodings_have_equal_hashes(int zoom, int x, int y)
    {
        var tile = new TileId(zoom, x, y);
        var restored = new TileId(tile.Id);
        var dictionary = new Dictionary<TileId, string> { [tile] = "tile" };
        Assert.Equal(tile.GetHashCode(), restored.GetHashCode());
        Assert.Equal("tile", dictionary[restored]);
        if (tile.TryGetPackedXY(out var packedXY))
        {
            var fromPackedXY = new TileId(zoom, packedXY);
            Assert.Equal(tile.GetHashCode(), fromPackedXY.GetHashCode());
            Assert.Equal("tile", dictionary[fromPackedXY]);
        }

        if (zoom == 0)
        {
            Assert.Equal(tile.GetHashCode(), default(TileId).GetHashCode());
            Assert.Equal("tile", dictionary[default]);
        }
    }

    [Theory]
    [InlineData(128, 1)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void Structured_coordinate_sequences_have_well_distributed_hashes(int xStep, int yStep)
    {
        const int count = 20_000;
        var hashes = new HashSet<int>();
        for (var index = 0; index < count; index++)
        {
            hashes.Add(new TileId(25, index * xStep, index * yStep).GetHashCode());
        }

        // Allow normal 32-bit collisions without accepting systematic clustering.
        // In particular, the old long hash collapses (128 * i, i) to one value.
        Assert.True(hashes.Count >= count * 99 / 100, $"Only {hashes.Count} distinct hashes for {count} tiles.");
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(25)]
    public void Rectangular_tile_grids_have_well_distributed_hashes(int zoom)
    {
        const int side = 256;
        var hashes = new HashSet<int>();
        for (var y = 0; y < side; y++)
        {
            for (var x = 0; x < side; x++)
            {
                hashes.Add(new TileId(zoom, x, y).GetHashCode());
            }
        }

        const int count = side * side;
        Assert.True(hashes.Count >= count * 99 / 100, $"Only {hashes.Count} distinct hashes for {count} tiles.");
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
