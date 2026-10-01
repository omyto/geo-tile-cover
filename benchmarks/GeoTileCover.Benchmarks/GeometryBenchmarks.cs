using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;

namespace GeoTileCover.Benchmarks;

// All methods include the input snapshot and start with empty TileCover caches.
[MemoryDiagnoser]
public class GeometryBenchmarks
{
    [ParamsAllValues]
    public GeometryWorkload Workload { get; set; }

    [Params(8, 16, 25)]
    public int Zoom { get; set; }

    private Geometry _geometry = null!;
    private int _expectedCount;
    private int _expectedMinimalCount;

    public int VertexCount => _geometry.NumPoints;
    public int TileCount => _expectedCount;
    public int MinimalTileCount => _expectedMinimalCount;

    [GlobalSetup]
    public void Setup()
    {
        _geometry = GeometryWorkloads.Create(Workload, Zoom);
        if (!_geometry.IsValid)
        {
            throw new InvalidOperationException($"Invalid benchmark geometry: {Workload} at zoom {Zoom}.");
        }

        // Bound the fixture before using either unbounded API.
        if (!new TileCover(_geometry).TryGetTiles(Zoom, GeometryWorkloads.TileBudget, out var expected))
        {
            throw new InvalidOperationException("Benchmark fixture exceeds its full-result budget.");
        }

        _expectedCount = expected.Length;
        if (_expectedCount <= GeometryWorkloads.RejectionBudget)
        {
            throw new InvalidOperationException("Benchmark fixture must exceed the rejection budget.");
        }

        var full = new TileCover(_geometry).GetTiles(Zoom).ToArray();
        var minimal = new TileCover(_geometry).GetMinimalTiles(0, Zoom);
        var expanded = new TileCover(minimal).GetTiles(Zoom).ToArray();
        if (!expected.SequenceEqual(full) || !expected.SequenceEqual(expanded))
        {
            throw new InvalidOperationException("Full, bounded, and minimal covers disagree.");
        }

        if (Workload == GeometryWorkload.FullTilePolygon && (full.Length != GeometryWorkloads.TileBudget || minimal.Length != 1))
        {
            throw new InvalidOperationException("The full-tile fixture must expand to 1,024 tiles and compact to one tile.");
        }

        _expectedMinimalCount = minimal.Length;
        _ = TryGetTilesRejected();
    }

    [Benchmark]
    public int GetTiles()
    {
        // Enumerate the complete result without introducing an extra array copy.
        var count = new TileCover(_geometry).GetTiles(Zoom).Count();
        if (count != _expectedCount)
        {
            throw new InvalidOperationException("Unexpected full cover size.");
        }

        return count;
    }

    [Benchmark]
    public TileId[] TryGetTilesComplete()
    {
        if (!new TileCover(_geometry).TryGetTiles(Zoom, GeometryWorkloads.TileBudget, out var tiles) || tiles.Length != _expectedCount)
        {
            throw new InvalidOperationException("Incomplete bounded cover.");
        }

        return tiles;
    }

    [Benchmark]
    public bool TryGetTilesRejected()
    {
        var success = new TileCover(_geometry).TryGetTiles(Zoom, GeometryWorkloads.RejectionBudget, out var tiles);
        if (success || tiles.Length != 0)
        {
            throw new InvalidOperationException("Expected rejection with an empty result.");
        }

        return success;
    }

    [Benchmark]
    public TileId[] GetMinimalTiles()
    {
        var tiles = new TileCover(_geometry).GetMinimalTiles(0, Zoom);
        if (tiles.Length != _expectedMinimalCount)
        {
            throw new InvalidOperationException("Unexpected minimal cover size.");
        }

        return tiles;
    }

    public static void ValidateWorkloads()
    {
        Console.WriteLine("Workload,Zoom,Vertices,FullTiles,MinimalTiles,RejectedAt");
        foreach (var workload in Enum.GetValues<GeometryWorkload>())
        {
            foreach (var zoom in GeometryWorkloads.Zooms)
            {
                var benchmark = new GeometryBenchmarks { Workload = workload, Zoom = zoom };
                benchmark.Setup();
                _ = benchmark.GetTiles();
                _ = benchmark.TryGetTilesComplete();
                _ = benchmark.TryGetTilesRejected();
                _ = benchmark.GetMinimalTiles();
                Console.WriteLine($"{workload},{zoom},{benchmark.VertexCount},{benchmark.TileCount},{benchmark.MinimalTileCount},{GeometryWorkloads.RejectionBudget}");
            }
        }
    }
}
