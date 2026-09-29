using System;
using NetTopologySuite.Geometries;

namespace GeoTileCover;

/// <summary>Identifies one Web Mercator XYZ tile.</summary>
public readonly struct TileId : IEquatable<TileId>
{
    /// <summary>The highest zoom level supported by the 32-bit packed X/Y representation.</summary>
    public const int PackedXYMaxZoom = 16;

    private const int CoordinateBits = TileCover.MaxZoom;
    private const int ZoomShift = CoordinateBits * 2;
    private const long CoordinateMask = (1L << CoordinateBits) - 1;

    /// <summary>Creates a tile from its XYZ coordinates.</summary>
    /// <param name="z">Zoom level from <see cref="TileCover.MinZoom"/> through <see cref="TileCover.MaxZoom"/>.</param>
    /// <param name="x">Zero-based tile column.</param>
    /// <param name="y">Zero-based tile row.</param>
    public TileId(int z, int x, int y)
    {
        var tilesPerAxis = TileMath.TilesPerAxis(z);
        if (x < 0 || x >= tilesPerAxis)
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }

        if (y < 0 || y >= tilesPerAxis)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        Z = z;
        X = x;
        Y = y;
    }

    /// <summary>Creates a tile from its zoom level and opaque 32-bit packed X/Y representation.</summary>
    /// <param name="z">Zoom level from 0 through 16.</param>
    /// <param name="packedXY">X and Y packed into <c>2 * z</c> bits. The value may be negative at zoom 16.</param>
    public TileId(int z, int packedXY)
    {
        var tilesPerAxis = TileMath.TilesPerAxis(z);
        if (z > PackedXYMaxZoom)
        {
            throw new ArgumentOutOfRangeException(nameof(z), $"Zoom must be {PackedXYMaxZoom} or lower when using packed XY.");
        }

        var bits = unchecked((uint)packedXY);
        var coordinateMask = (1u << z) - 1;
        var x = bits >> z;
        var y = bits & coordinateMask;

        // Validate before narrowing: at zoom 0, a negative packed value still has its sign bit set.
        if (x >= (uint)tilesPerAxis || y >= (uint)tilesPerAxis)
        {
            throw new ArgumentOutOfRangeException(nameof(packedXY), "Packed XY contains coordinates outside the encoded zoom grid.");
        }

        Z = z;
        X = (int)x;
        Y = (int)y;
    }

    /// <summary>Creates a tile from its packed numeric ID.</summary>
    public TileId(long id)
    {
        var z = (int)(id >> ZoomShift);
        var x = (int)((id >> CoordinateBits) & CoordinateMask);
        var y = (int)(id & CoordinateMask);

        if (id < 0 || z < TileCover.MinZoom || z > TileCover.MaxZoom)
        {
            throw new ArgumentOutOfRangeException(nameof(id), $"ID must encode a zoom between {TileCover.MinZoom} and {TileCover.MaxZoom}.");
        }

        var tilesPerAxis = TileMath.TilesPerAxis(z);
        if (x >= tilesPerAxis || y >= tilesPerAxis)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "ID contains tile coordinates outside the encoded zoom grid.");
        }

        Z = z;
        X = x;
        Y = y;
    }

    /// <summary>Gets the zoom level.</summary>
    public int Z { get; }

    /// <summary>Gets the zero-based tile column.</summary>
    public int X { get; }

    /// <summary>Gets the zero-based tile row.</summary>
    public int Y { get; }

    /// <summary>Gets the opaque 32-bit packed X/Y representation, unique within <see cref="Z"/>. The value may be negative at zoom 16.</summary>
    /// <exception cref="InvalidOperationException"><see cref="Z"/> is greater than 16.</exception>
    public int PackedXY
    {
        get
        {
            if (!TryGetPackedXY(out var packedXY))
            {
                throw new InvalidOperationException($"Packed XY is only available through zoom {PackedXYMaxZoom}.");
            }

            return packedXY;
        }
    }

    /// <summary>Attempts to get the opaque 32-bit packed X/Y representation.</summary>
    /// <param name="packedXY">When this method returns true, contains X and Y packed into <c>2 * Z</c> bits.</param>
    /// <returns>True when <see cref="Z"/> is 16 or lower; otherwise, false.</returns>
    public bool TryGetPackedXY(out int packedXY)
    {
        if (Z > PackedXYMaxZoom)
        {
            packedXY = default;
            return false;
        }

        packedXY = unchecked((int)(((uint)X << Z) | (uint)Y));
        return true;
    }

    /// <summary>Gets the stable, collision-free numeric ID packed as 5-bit Z, 25-bit X, and 25-bit Y.</summary>
    public long Id => ((long)Z << ZoomShift) | ((long)X << CoordinateBits) | (uint)Y;

    /// <summary>Gets the parent tile at the preceding zoom level.</summary>
    public TileId Parent()
    {
        if (Z == 0)
        {
            throw new InvalidOperationException("Zoom 0 tile has no parent.");
        }

        return new TileId(Z - 1, X >> 1, Y >> 1);
    }

    /// <summary>Creates the WGS84 geographic bounds of this tile, with X as longitude and Y as latitude.</summary>
    public Envelope ToEnvelope() => TileMath.ToEnvelope(this);

    /// <inheritdoc/>
    public bool Equals(TileId other) => Z == other.Z && X == other.X && Y == other.Y;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TileId other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        // MurmurHash3 fmix64 mixes the packed fields before reducing to 32 bits.
        // https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp
        unchecked
        {
            var hash = (ulong)Id;
            hash ^= hash >> 33;
            hash *= 0xff51afd7ed558ccdUL;
            hash ^= hash >> 33;
            hash *= 0xc4ceb9fe1a85ec53UL;
            hash ^= hash >> 33;
            return (int)hash;
        }
    }

    /// <summary>Returns the tile coordinate formatted as <c>z/x/y</c>.</summary>
    public override string ToString() => $"{Z}/{X}/{Y}";

    /// <summary>Determines whether two tile identifiers are equal.</summary>
    public static bool operator ==(TileId left, TileId right) => left.Equals(right);

    /// <summary>Determines whether two tile identifiers are different.</summary>
    public static bool operator !=(TileId left, TileId right) => !left.Equals(right);
}
