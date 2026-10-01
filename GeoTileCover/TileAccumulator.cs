using System;
using System.Collections.Generic;
using System.Threading;

namespace GeoTileCover;

internal sealed class TileAccumulator
{
    private readonly HashSet<TileId> _tiles = new HashSet<TileId>();
    private readonly int? _maxTiles;
    private readonly CancellationToken _cancellationToken;

    public TileAccumulator(int? maxTiles = null, CancellationToken cancellationToken = default)
    {
        _maxTiles = maxTiles;
        _cancellationToken = cancellationToken;
    }

    public IEnumerable<TileId> Tiles => _tiles;

    public void CheckCancellation() => _cancellationToken.ThrowIfCancellationRequested();

    // Avoid extra coverage predicates for tiny bounded requests.
    // Require a budget for two levels of descendants before paying for Covers.
    // Use the total budget: overlapping components may add the same tiles again.
    public bool UseCoveredBranchFastPath => !_maxTiles.HasValue || _maxTiles.Value >= 16;

    public bool TryAdd(TileId tile)
    {
        CheckCancellation();
        _tiles.Add(tile);
        return !_maxTiles.HasValue || _tiles.Count <= _maxTiles.Value;
    }

    public bool TryAddDescendants(TileId tile, int targetZoom)
    {
        CheckCancellation();
        if (tile.Z == targetZoom)
        {
            return TryAdd(tile);
        }

        var z = tile.Z + 1;
        var x = tile.X << 1;
        var y = tile.Y << 1;
        return TryAddDescendants(new TileId(z, x, y), targetZoom)
            && TryAddDescendants(new TileId(z, x + 1, y), targetZoom)
            && TryAddDescendants(new TileId(z, x, y + 1), targetZoom)
            && TryAddDescendants(new TileId(z, x + 1, y + 1), targetZoom);
    }

    public TileId[] ToArray()
    {
        CheckCancellation();
        var ordered = new List<TileId>(_tiles);
        ordered.Sort(TileIdComparer.Instance);
        CheckCancellation();
        return ordered.ToArray();
    }
}
