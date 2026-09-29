# GeoTileCover benchmarks

This standalone .NET 10 console project uses BenchmarkDotNet. Its package reference is confined to the benchmark project; GeoTileCover keeps its existing dependencies and `netstandard2.0` target.

Run from the repository root in Release mode:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*' --job short --artifacts artifacts/hash-benchmarks
```

For a shorter comparison with explicit sampling settings:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*' --job short --iterationCount 7 --warmupCount 3 --iterationTime 100 --artifacts artifacts/hash-benchmarks --join
```

Filter individual groups with `--filter '*HashBenchmarks*'`, `--filter '*HashSetBenchmarks*'`, or `--filter '*CoverBenchmarks*'`. BenchmarkDotNet reports runtime, CPU, confidence intervals, allocations, and any measurement warnings alongside the results. Use its longer default job when small differences could affect a decision; repeat on target runtimes and hardware before generalizing.

Generate the deterministic hash distribution report separately:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --distribution
```

A [recorded comparison](RESULTS.md) includes raw results and explains why the library retains Murmur low-32.

## What is measured

- `HashBenchmarks`: amortized throughput for 16,384 precomputed packed IDs. Methods return a checksum to keep their work observable. ID construction, geometry operations, and collection allocations are excluded.
- `HashSetBenchmarks`: build and successful lookup of 16,384 tiles, using the same equality and comparer structure for every candidate. Inputs cover correlated coordinates `(128*i, i)`, a rectangular grid, and deterministic random coordinates. These comparisons use candidate comparers, not replacements of the production `TileId` method.
- `CoverBenchmarks`: the actual library with its current hash implementation. Measures construction from tiles and a fresh geometry cover plus `TryGetTiles` from point centers. Every invocation starts without a tile cache. These results provide an end-to-end baseline, not an end-to-end comparison of the candidate hashes.
- `--distribution`: 65,536 distinct tiles per dataset, including rows, columns, diagonals, grids, correlated coordinates, and four random seeds. Reports distinct hashes, extra keys sharing a hash, and the largest group sharing one hash. This is a regression-oriented sample, not an exhaustive proof of hash quality or a cryptographic test.

## Candidate hashes

`LongXor` reproduces the previous `Id.GetHashCode()` implementation. The other four candidates apply either MurmurHash3 `fmix64` or classic `XXH64_avalanche` to the packed ID, then take the low 32 bits or XOR the two 32-bit halves. This compares finalizers only, not the complete MurmurHash3, xxHash64, or XXH3 byte hashing algorithms.

For an already well-mixed 64-bit result, both reductions still produce only 32 bits. XOR folding is not automatically better. In particular, `XXH64_avalanche` ends with `h ^= h >> 32`; XOR folding its output cancels that final step when producing the low 32 bits.

Normal collisions are expected: for 65,536 independent, uniformly distributed 32-bit hashes, the expected number of colliding pairs is about 0.5. A difference of a few collisions on one dataset is not evidence of superiority. Avoid selecting a candidate solely because one seed happens to produce no collisions, or solely from hashing throughput when collection performance is comparable.

References: [MurmurHash3 fmix64](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp), [xxHash v0.8.3 avalanche](https://github.com/Cyan4973/xxHash/blob/v0.8.3/xxhash.h). See [third-party notices](THIRD-PARTY-NOTICES.md) for the benchmark's copied finalizers.
