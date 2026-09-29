using System.Runtime.CompilerServices;

namespace GeoTileCover.Benchmarks;

public enum HashVariant
{
    LongXor,
    MurmurLow32,
    MurmurFold32,
    XxHashLow32,
    XxHashFold32
}

internal static class HashMixers
{
    // MurmurHash3 fmix64, public domain, Austin Appleby.
    // https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Murmur64(ulong hash)
    {
        unchecked
        {
            hash ^= hash >> 33;
            hash *= 0xff51afd7ed558ccdUL;
            hash ^= hash >> 33;
            hash *= 0xc4ceb9fe1a85ec53UL;
            hash ^= hash >> 33;
            return hash;
        }
    }

    // Only XXH64_avalanche, not the full xxHash64 byte hashing algorithm.
    // Adapted from xxHash v0.8.3, Copyright (C) 2012-2023 Yann Collet.
    // BSD-2-Clause; see THIRD-PARTY-NOTICES.md.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XxHash64(ulong hash)
    {
        unchecked
        {
            hash ^= hash >> 33;
            hash *= 0xc2b2ae3d27d4eb4fUL;
            hash ^= hash >> 29;
            hash *= 0x165667b19e3779f9UL;
            hash ^= hash >> 32;
            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Fold(ulong hash) => unchecked((int)(hash ^ (hash >> 32)));
}

internal static class HashComparers
{
    public static IEqualityComparer<TileId> Create(HashVariant variant) => variant switch
    {
        HashVariant.LongXor => new LongXorComparer(),
        HashVariant.MurmurLow32 => new MurmurLowComparer(),
        HashVariant.MurmurFold32 => new MurmurFoldComparer(),
        HashVariant.XxHashLow32 => new XxHashLowComparer(),
        HashVariant.XxHashFold32 => new XxHashFoldComparer(),
        _ => throw new ArgumentOutOfRangeException(nameof(variant))
    };

    private abstract class TileComparer : IEqualityComparer<TileId>
    {
        public bool Equals(TileId x, TileId y) => x.Equals(y);
        public abstract int GetHashCode(TileId tile);
    }

    private sealed class LongXorComparer : TileComparer
    {
        public override int GetHashCode(TileId tile) => tile.Id.GetHashCode();
    }

    private sealed class MurmurLowComparer : TileComparer
    {
        public override int GetHashCode(TileId tile) => unchecked((int)HashMixers.Murmur64((ulong)tile.Id));
    }

    private sealed class MurmurFoldComparer : TileComparer
    {
        public override int GetHashCode(TileId tile) => HashMixers.Fold(HashMixers.Murmur64((ulong)tile.Id));
    }

    private sealed class XxHashLowComparer : TileComparer
    {
        public override int GetHashCode(TileId tile) => unchecked((int)HashMixers.XxHash64((ulong)tile.Id));
    }

    private sealed class XxHashFoldComparer : TileComparer
    {
        public override int GetHashCode(TileId tile) => HashMixers.Fold(HashMixers.XxHash64((ulong)tile.Id));
    }
}
