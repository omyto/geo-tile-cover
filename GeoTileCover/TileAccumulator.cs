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

    public bool TryAdd(TileId tile)
    {
        CheckCancellation();
        _tiles.Add(tile);
        return !_maxTiles.HasValue || _tiles.Count <= _maxTiles.Value;
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
