using System.Collections.Generic;

namespace GeoTileCover;

/// <summary>Accumulates coverage at one resolution, compacting complete subtrees as they are filled.</summary>
internal sealed class MinimalTileTree
{
    public Node Root { get; } = new Node();

    public void Add(TileId tile) => Add(Root, tile, 0);

    public TileId[] ToArray(int minZoom)
    {
        var tiles = new List<TileId>();
        Collect(Root, new TileId(0, 0, 0), minZoom, tiles);
        tiles.Sort(TileIdComparer.Instance);
        return tiles.ToArray();
    }

    private static void Add(Node node, TileId tile, int depth)
    {
        if (node.IsFull)
        {
            return;
        }

        if (depth == tile.Z)
        {
            node.Fill();
            return;
        }

        var shift = tile.Z - depth - 1;
        var quadrant = ((tile.Y >> shift) & 1) * 2 + ((tile.X >> shift) & 1);
        Add(node.GetOrCreateChild(quadrant), tile, depth + 1);
        node.Compact();
    }

    private static void Collect(Node node, TileId tile, int minZoom, List<TileId> tiles)
    {
        if (node.IsFull)
        {
            AddAtMinZoom(tile, minZoom, tiles);
            return;
        }

        for (var quadrant = 0; quadrant < 4; quadrant++)
        {
            var child = node.GetChild(quadrant);
            if (child != null)
            {
                Collect(child, ChildTile(tile, quadrant), minZoom, tiles);
            }
        }
    }

    private static void AddAtMinZoom(TileId tile, int minZoom, List<TileId> tiles)
    {
        if (tile.Z >= minZoom)
        {
            tiles.Add(tile);
            return;
        }

        for (var quadrant = 0; quadrant < 4; quadrant++)
        {
            AddAtMinZoom(ChildTile(tile, quadrant), minZoom, tiles);
        }
    }

    public static TileId ChildTile(TileId tile, int quadrant) =>
        new TileId(tile.Z + 1, (tile.X << 1) + (quadrant & 1), (tile.Y << 1) + (quadrant >> 1));

    internal sealed class Node
    {
        private Node?[]? _children;

        public bool IsFull { get; private set; }

        public Node? GetChild(int quadrant) => _children?[quadrant];

        public Node GetOrCreateChild(int quadrant)
        {
            _children ??= new Node?[4];
            return _children[quadrant] ??= new Node();
        }

        public void Fill()
        {
            IsFull = true;
            _children = null;
        }

        public void Compact()
        {
            if (_children == null)
            {
                return;
            }

            var fullCount = 0;
            var nonemptyCount = 0;
            for (var quadrant = 0; quadrant < 4; quadrant++)
            {
                var child = _children[quadrant];
                if (child == null || (!child.IsFull && child._children == null))
                {
                    _children[quadrant] = null;
                    continue;
                }

                nonemptyCount++;
                if (child.IsFull)
                {
                    fullCount++;
                }
            }

            if (fullCount == 4)
            {
                Fill();
            }
            else if (nonemptyCount == 0)
            {
                _children = null;
            }
        }
    }
}
