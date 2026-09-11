using System;
using System.Collections.Generic;

namespace GeoTileCover;

/// <summary>Stores a union of fully covered XYZ tiles in a compact canonical quadtree representation.</summary>
internal sealed class CanonicalTileUnion
{
    private readonly TileId[] _tiles;

    public CanonicalTileUnion(IEnumerable<TileId> tiles)
    {
        if (tiles == null)
        {
            throw new ArgumentNullException(nameof(tiles));
        }

        _tiles = Normalize(tiles, TileCover.MinZoom, TileCover.MaxZoom);
    }

    public TileId[] GetTiles(int zoom)
    {
        var result = new HashSet<TileId>();
        foreach (var tile in _tiles)
        {
            if (tile.Z < zoom)
            {
                AddDescendants(tile, zoom, result);
            }
            else
            {
                result.Add(GetAncestor(tile, zoom));
            }
        }

        return Sort(result);
    }

    public TileId[] GetMinimalTiles(int minZoom, int maxZoom)
    {
        var bounded = new HashSet<TileId>();
        foreach (var tile in _tiles)
        {
            if (tile.Z < minZoom)
            {
                AddDescendants(tile, minZoom, bounded);
            }
            else if (tile.Z > maxZoom)
            {
                bounded.Add(GetAncestor(tile, maxZoom));
            }
            else
            {
                bounded.Add(tile);
            }
        }

        return Normalize(bounded, minZoom, maxZoom);
    }

    private static TileId[] Normalize(IEnumerable<TileId> tiles, int minZoom, int maxZoom)
    {
        var candidates = new List<TileId>(new HashSet<TileId>(tiles));
        candidates.Sort(TileIdComparer.Instance);

        var normalized = new HashSet<TileId>();
        foreach (var tile in candidates)
        {
            if (!HasAncestor(normalized, tile))
            {
                normalized.Add(tile);
            }
        }

        for (var zoom = maxZoom; zoom > minZoom; zoom--)
        {
            CompactSiblings(normalized, zoom);
        }

        return Sort(normalized);
    }

    private static void AddDescendants(TileId tile, int targetZoom, HashSet<TileId> result)
    {
        if (tile.Z == targetZoom)
        {
            result.Add(tile);
            return;
        }

        var z = tile.Z + 1;
        var x = tile.X << 1;
        var y = tile.Y << 1;
        AddDescendants(new TileId(z, x, y), targetZoom, result);
        AddDescendants(new TileId(z, x + 1, y), targetZoom, result);
        AddDescendants(new TileId(z, x, y + 1), targetZoom, result);
        AddDescendants(new TileId(z, x + 1, y + 1), targetZoom, result);
    }

    private static TileId GetAncestor(TileId tile, int targetZoom)
    {
        while (tile.Z > targetZoom)
        {
            tile = tile.Parent();
        }

        return tile;
    }

    private static bool HasAncestor(HashSet<TileId> tiles, TileId tile)
    {
        while (tile.Z > TileCover.MinZoom)
        {
            tile = tile.Parent();
            if (tiles.Contains(tile))
            {
                return true;
            }
        }

        return false;
    }

    private static void CompactSiblings(HashSet<TileId> tiles, int zoom)
    {
        var childCounts = new Dictionary<TileId, int>();
        foreach (var tile in tiles)
        {
            if (tile.Z != zoom)
            {
                continue;
            }

            var parent = tile.Parent();
            childCounts.TryGetValue(parent, out var count);
            childCounts[parent] = count + 1;
        }

        foreach (var pair in childCounts)
        {
            if (pair.Value != 4)
            {
                continue;
            }

            var parent = pair.Key;
            var childZoom = parent.Z + 1;
            var childX = parent.X << 1;
            var childY = parent.Y << 1;
            tiles.Remove(new TileId(childZoom, childX, childY));
            tiles.Remove(new TileId(childZoom, childX + 1, childY));
            tiles.Remove(new TileId(childZoom, childX, childY + 1));
            tiles.Remove(new TileId(childZoom, childX + 1, childY + 1));
            tiles.Add(parent);
        }
    }

    private static TileId[] Sort(HashSet<TileId> tiles)
    {
        var ordered = new List<TileId>(tiles);
        ordered.Sort(TileIdComparer.Instance);
        return ordered.ToArray();
    }
}
