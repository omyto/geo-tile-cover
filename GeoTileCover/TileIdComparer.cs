using System.Collections.Generic;

namespace GeoTileCover;

internal sealed class TileIdComparer : IComparer<TileId>
{
    public static readonly TileIdComparer Instance = new TileIdComparer();

    public int Compare(TileId left, TileId right)
    {
        var byZoom = left.Z.CompareTo(right.Z);
        if (byZoom != 0)
        {
            return byZoom;
        }

        var byY = left.Y.CompareTo(right.Y);
        return byY != 0 ? byY : left.X.CompareTo(right.X);
    }
}
