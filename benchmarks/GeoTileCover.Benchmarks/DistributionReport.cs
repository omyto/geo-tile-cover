namespace GeoTileCover.Benchmarks;

internal static class DistributionReport
{
    public static void Write()
    {
        Console.WriteLine("Pattern,Seed,Variant,Count,DistinctHashes,CollidingKeys,MaxMultiplicity");
        foreach (var pattern in Enum.GetValues<TilePattern>())
        {
            // More than one deterministic random sample avoids selecting by one lucky seed.
            var seeds = pattern == TilePattern.Random ? new[] { 372, 7711, 173, 2026 } : new[] { 372 };
            foreach (var seed in seeds)
            {
                var tiles = TileData.Create(pattern, 65_536, seed);
                foreach (var variant in Enum.GetValues<HashVariant>())
                {
                    var comparer = HashComparers.Create(variant);
                    var groups = new Dictionary<int, int>();
                    foreach (var tile in tiles)
                    {
                        var hash = comparer.GetHashCode(tile);
                        groups.TryGetValue(hash, out var count);
                        groups[hash] = count + 1;
                    }
                    Console.WriteLine($"{pattern},{seed},{variant},{tiles.Length},{groups.Count},{tiles.Length - groups.Count},{groups.Values.Max()}");
                }
            }
        }
    }
}
