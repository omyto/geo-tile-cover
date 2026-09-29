using BenchmarkDotNet.Attributes;

namespace GeoTileCover.Benchmarks;

[MemoryDiagnoser]
public class HashBenchmarks
{
    private const int Count = 16_384;
    private ulong[] _ids = null!;

    [GlobalSetup]
    public void Setup() => _ids = TileData.Create(TilePattern.Random, Count).Select(tile => (ulong)tile.Id).ToArray();

    [Benchmark(Baseline = true, OperationsPerInvoke = Count)]
    public int LongXor()
    {
        var sum = 0;
        foreach (var id in _ids)
        {
            sum = unchecked(sum + HashMixers.Fold(id));
        }
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Count)]
    public int MurmurLow32()
    {
        var sum = 0;
        foreach (var id in _ids)
        {
            sum = unchecked(sum + (int)HashMixers.Murmur64(id));
        }
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Count)]
    public int MurmurFold32()
    {
        var sum = 0;
        foreach (var id in _ids)
        {
            sum = unchecked(sum + HashMixers.Fold(HashMixers.Murmur64(id)));
        }
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Count)]
    public int XxHashLow32()
    {
        var sum = 0;
        foreach (var id in _ids)
        {
            sum = unchecked(sum + (int)HashMixers.XxHash64(id));
        }
        return sum;
    }

    [Benchmark(OperationsPerInvoke = Count)]
    public int XxHashFold32()
    {
        var sum = 0;
        foreach (var id in _ids)
        {
            sum = unchecked(sum + HashMixers.Fold(HashMixers.XxHash64(id)));
        }
        return sum;
    }
}

[MemoryDiagnoser]
public class HashSetBenchmarks
{
    [Params(HashVariant.LongXor, HashVariant.MurmurLow32, HashVariant.MurmurFold32, HashVariant.XxHashLow32, HashVariant.XxHashFold32)]
    public HashVariant Variant { get; set; }

    [Params(TilePattern.Correlated, TilePattern.Grid, TilePattern.Random)]
    public TilePattern Pattern { get; set; }

    private TileId[] _tiles = null!;
    private IEqualityComparer<TileId> _comparer = null!;
    private HashSet<TileId> _set = null!;

    [GlobalSetup]
    public void Setup()
    {
        _tiles = TileData.Create(Pattern, 16_384);
        _comparer = HashComparers.Create(Variant);
        _set = new HashSet<TileId>(_tiles, _comparer);
        if (_set.Count != _tiles.Length)
        {
            throw new InvalidOperationException("Benchmark data contains duplicate tiles.");
        }
    }

    [Benchmark]
    public HashSet<TileId> Build() => new HashSet<TileId>(_tiles, _comparer);

    [Benchmark]
    public int Contains()
    {
        var found = 0;
        foreach (var tile in _tiles)
        {
            if (_set.Contains(tile))
            {
                found++;
            }
        }
        return found;
    }
}
