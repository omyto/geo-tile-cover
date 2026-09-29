using BenchmarkDotNet.Attributes;
using NetTopologySuite.Geometries;

namespace GeoTileCover.Benchmarks;

// Uses the actual library hash. Candidate comparers are isolated in HashSetBenchmarks.
[MemoryDiagnoser]
public class CoverBenchmarks
{
    [Params(TilePattern.Correlated, TilePattern.Grid, TilePattern.Random)]
    public TilePattern Pattern { get; set; }

    private TileId[] _tiles = null!;
    private Geometry _points = null!;

    [GlobalSetup]
    public void Setup()
    {
        _tiles = TileData.Create(Pattern, 16_384);
        var coordinates = _tiles.Select(tile =>
        {
            var bounds = tile.ToEnvelope();
            return new Coordinate((bounds.MinX + bounds.MaxX) / 2, (bounds.MinY + bounds.MaxY) / 2);
        }).ToArray();
        _points = new GeometryFactory().CreateMultiPointFromCoords(coordinates);
    }

    [Benchmark]
    public TileCover FromTiles() => new TileCover(_tiles);

    [Benchmark]
    public TileId[] FromPoints()
    {
        var cover = new TileCover(_points);
        if (!cover.TryGetTiles(25, _tiles.Length, out var tiles) || tiles.Length != _tiles.Length)
        {
            throw new InvalidOperationException("Incomplete point coverage.");
        }
        return tiles;
    }
}
