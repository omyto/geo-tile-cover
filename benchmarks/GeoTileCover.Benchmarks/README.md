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

Filter individual groups with `--filter '*HashBenchmarks*'`, `--filter '*HashSetBenchmarks*'`, `--filter '*CoverBenchmarks*'`, or `--filter '*GeometryBenchmarks*'`. BenchmarkDotNet reports runtime, CPU, confidence intervals, allocations, and any measurement warnings alongside the results. Use its longer default job when small differences could affect a decision; repeat on target runtimes and hardware before generalizing.

Generate the deterministic hash distribution report separately:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --distribution
```

A [recorded comparison](RESULTS.md) includes raw results and explains why the library retains Murmur low-32.

## What is measured

- `HashBenchmarks`: amortized throughput for 16,384 precomputed packed IDs. Methods return a checksum to keep their work observable. ID construction, geometry operations, and collection allocations are excluded.
- `HashSetBenchmarks`: build and successful lookup of 16,384 tiles, using the same equality and comparer structure for every candidate. Inputs cover correlated coordinates `(128*i, i)`, a rectangular grid, and deterministic random coordinates. These comparisons use candidate comparers, not replacements of the production `TileId` method.
- `CoverBenchmarks`: the actual library with its current hash implementation. Measures construction from tiles and a fresh geometry cover plus `TryGetTiles` from point centers. Every invocation starts without a tile cache. These results provide an end-to-end baseline, not an end-to-end comparison of the candidate hashes.
- `GeometryBenchmarks`: cold-cache coverage of lines and polygons, including complete and rejected bounded requests. See the workload and measurement details below.
- `--distribution`: 65,536 distinct tiles per dataset, including rows, columns, diagonals, grids, correlated coordinates, and four random seeds. Reports distinct hashes, extra keys sharing a hash, and the largest group sharing one hash. This is a regression-oriented sample, not an exhaustive proof of hash quality or a cryptographic test.

## Geometry coverage

The [2026-10-01 exploratory baseline](GEOMETRY-RESULTS.md) records all 48 cases, fixture output counts, and measurement limitations.

The [covered polygon branch comparison](COVERED-BRANCH-RESULTS.md) measures the subsequent fast path against the original implementation with longer warmup, including line and rejected-request controls.

Validate every fixture and exercise all four methods before measuring:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --validate-geometry
```

This reports vertex counts and full/minimal output counts. Setup checks topology, bounds the full cover to 1,024 tiles, compares `GetTiles` with successful `TryGetTiles`, and expands the minimal result to verify identical coverage. These checks run outside the measured methods.

Run the 48 cases (four workloads, three zooms, four methods):

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*GeometryBenchmarks*' --job short --artifacts artifacts/geometry-benchmarks
```

For an exploratory run with fewer samples:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*GeometryBenchmarks*' --job short --launchCount 1 --warmupCount 1 --iterationCount 3 --iterationTime 100 --artifacts artifacts/geometry-benchmarks-short
```

| Workload | Input | Purpose |
|---|---|---|
| `FullTilePolygon` | Five coordinates forming one exact ancestor tile | Exposes work inside fully covered branches; produces 1,024 full tiles or one minimal tile. |
| `DetailedLine` | 2,049 coordinates along a sinusoidal line | Exercises expensive line traversal without polygon interiors to shortcut. |
| `PolygonWithHole` | 2,048 outer-ring vertices and 512 hole vertices, plus closing coordinates | Combines covered interior, curved boundaries, and an excluded hole. |
| `MultiPolygon` | Four disjoint polygons, each with 512 vertices plus closure | Exercises traversal across components with curved boundaries and covered interiors. |

All fixtures fit inside one tile five levels above the requested zoom, bounding output to 32 x 32 tiles. Zooms 8, 16, and 25 use the same vertex counts and normalized shapes, rescaled in longitude/latitude to that tile's envelope. This controls output size at high zoom; it does **not** measure a fixed geographic region whose tile count grows exponentially. Mercator distortion means exact output counts can differ by zoom. The coordinates are deterministic synthetic data, not real-world geographic datasets.

Each timed invocation creates a new `TileCover`, including its geometry snapshot and validation of coordinate bounds, then invokes the API. Fixture construction and topology validation are excluded. Nothing reuses a warm result cache. `GetTiles` enumerates the complete result with `Count()` without an extra `ToArray()` copy; the other successful methods return their API arrays. `MemoryDiagnoser` reports allocations for the whole invocation.

- `GetTiles` and `TryGetTilesComplete` return the same full coverage; the latter has a 1,024-tile budget.
- `TryGetTilesRejected` has an eight-tile budget and must return `false` with an empty array. It measures early termination, so its runtime is not a full-cover comparison.
- `GetMinimalTiles` uses the range `0..Zoom`. It returns a different, compact representation, so its runtime and allocations are not a drop-in performance ratio against full enumeration.

Use the same fixtures before and after any optimization, including line and rejected-request controls. The initial baseline predates the `Covers` fast path; the linked comparison measures that change. Neither run measures a minimal-then-expand algorithm. Short-run differences require longer repeated runs before making an implementation decision.

## Candidate hashes

`LongXor` reproduces the previous `Id.GetHashCode()` implementation. The other four candidates apply either MurmurHash3 `fmix64` or classic `XXH64_avalanche` to the packed ID, then take the low 32 bits or XOR the two 32-bit halves. This compares finalizers only, not the complete MurmurHash3, xxHash64, or XXH3 byte hashing algorithms.

For an already well-mixed 64-bit result, both reductions still produce only 32 bits. XOR folding is not automatically better. In particular, `XXH64_avalanche` ends with `h ^= h >> 32`; XOR folding its output cancels that final step when producing the low 32 bits.

Normal collisions are expected: for 65,536 independent, uniformly distributed 32-bit hashes, the expected number of colliding pairs is about 0.5. A difference of a few collisions on one dataset is not evidence of superiority. Avoid selecting a candidate solely because one seed happens to produce no collisions, or solely from hashing throughput when collection performance is comparable.

References: [MurmurHash3 fmix64](https://github.com/aappleby/smhasher/blob/master/src/MurmurHash3.cpp), [xxHash v0.8.3 avalanche](https://github.com/Cyan4973/xxHash/blob/v0.8.3/xxhash.h). See [third-party notices](THIRD-PARTY-NOTICES.md) for the benchmark's copied finalizers.
