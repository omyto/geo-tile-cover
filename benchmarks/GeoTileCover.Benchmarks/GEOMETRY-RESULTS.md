# Geometry coverage baseline, 2026-10-01

The geometry benchmark suite completed all 48 cases against the unchanged library at commit `e8d753631baa85ccc9d6956c74f459e2f79da5c0`, with the new benchmark sources added in the working tree. All 12 fixtures passed topology, tile-budget, full/bounded equality, and minimal-cover expansion checks. [Fixture counts](results/2026-10-01/geometry-workloads.csv), [BenchmarkDotNet report](results/2026-10-01/geometry-benchmark-report.md), and [CSV](results/2026-10-01/geometry-benchmark-report.csv) are retained for reproduction.

## Run conditions

BenchmarkDotNet 0.15.8, .NET 10.0.12, SDK 10.0.401, Windows x64, Intel Core i5-10400. One launch, one warmup iteration, three measured iterations, and a 100 ms target iteration time:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*GeometryBenchmarks*' --job short --launchCount 1 --warmupCount 1 --iterationCount 3 --iterationTime 100 --artifacts artifacts/geometry-benchmarks-short
```

This is an exploratory baseline and a check that every benchmark runs, not a stable performance ranking. Several polygon measurements changed substantially across the three samples; some 99.9% confidence intervals are wider than their means. BenchmarkDotNet also warned that many observed iteration times were below 100 ms. Use longer warmup and measurement runs, and repeat on the target environment before deciding on an optimization or reporting a speedup.

## What this establishes

- The full-tile fixture returns 1,024 tiles from both full-cover methods and one tile from `GetMinimalTiles`. It isolates a useful case for testing a future fully covered branch shortcut, but the minimal result has a different output contract and is not evidence of a measured full-cover speedup.
- Minimal traversal is not uniformly cheaper. For the detailed line at zoom 25, the observed means were approximately 365 ms for `GetTiles` and 595 ms for `GetMinimalTiles`, with approximately 233 MiB and 376 MiB allocated per invocation respectively. Calling minimal traversal before expanding is therefore not an established general replacement for direct enumeration. This suite does not time the minimal-then-expand composition itself.
- The rejected request stops at an eight-tile budget and returns an empty result. Preserve that early-exit behavior when evaluating any alternative traversal.
- Polygon and MultiPolygon fixtures include roughly 2,000 vertices, curved boundaries, and (for the polygon) a hole. Their noisy short-run timings do not establish whether a new `Covers` check would pay for itself.

All timed calls start with a new `TileCover`, including input copying. The footprint scales with zoom to keep full output at or below 1,024 tiles; these are synthetic complexity tests at zooms 8, 16, and 25, not fixed-area high-zoom expansion tests or warm-cache measurements. See the [benchmark guide](README.md#geometry-coverage) for the exact workloads and API measurement contracts.

No production optimization was made. The next decision requires comparing a candidate against the same full-result workloads, including allocation cost and regressions in line and rejected-request cases.
