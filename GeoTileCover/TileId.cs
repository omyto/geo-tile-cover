using System;

namespace GeoTileCover;

/// <summary>Identifies one Web Mercator XYZ tile.</summary>
public readonly struct TileId : IEquatable<TileId>
{
    private const int CoordinateBits = TileCover.MaxZoom;
    private const int ZoomShift = CoordinateBits * 2;
    private const long CoordinateMask = (1L << CoordinateBits) - 1;

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

    public int Z { get; }
    public int X { get; }
    public int Y { get; }

    /// <summary>Gets the stable, collision-free numeric ID packed as 5-bit Z, 25-bit X, and 25-bit Y.</summary>
    public long Id => ((long)Z << ZoomShift) | ((long)X << CoordinateBits) | (uint)Y;

    public TileId Parent()
    {
        if (Z == 0)
        {
            throw new InvalidOperationException("Zoom 0 tile has no parent.");
        }

        return new TileId(Z - 1, X >> 1, Y >> 1);
    }

    public bool Equals(TileId other) => Z == other.Z && X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is TileId other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"{Z}/{X}/{Y}";
    public static bool operator ==(TileId left, TileId right) => left.Equals(right);
    public static bool operator !=(TileId left, TileId right) => !left.Equals(right);
}
