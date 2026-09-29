namespace GeoTileCover.Benchmarks;

public enum TilePattern
{
    Correlated,
    Grid,
    Row,
    Column,
    Diagonal,
    Random
}

internal static class TileData
{
    public static TileId[] Create(TilePattern pattern, int count, int seed = 372)
    {
        var random = new Random(seed);
        var tiles = new TileId[count];
        var seen = new HashSet<long>();
        for (var index = 0; index < count; index++)
        {
            TileId tile;
            do
            {
                tile = pattern switch
                {
                    TilePattern.Correlated => new TileId(25, 128 * index, index),
                    TilePattern.Grid => new TileId(25, index % 256, index / 256),
                    TilePattern.Row => new TileId(25, index, 0),
                    TilePattern.Column => new TileId(25, 0, index),
                    TilePattern.Diagonal => new TileId(25, index, index),
                    TilePattern.Random => new TileId(25, random.Next(1 << 25), random.Next(1 << 25)),
                    _ => throw new ArgumentOutOfRangeException(nameof(pattern))
                };
            }
            while (pattern == TilePattern.Random && !seen.Add(tile.Id));
            tiles[index] = tile;
        }

        return tiles;
    }
}
