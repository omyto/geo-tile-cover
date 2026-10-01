# Fully covered polygon branches, 2026-10-01

Retain the polygon fast path for geometry-backed full-cover APIs. In the measured fixtures it substantially reduces runtime and allocations for polygons while preserving complete tile output. Tiny bounded requests retain ordinary traversal. This is not a change to the geometry-backed `GetMinimalTiles` algorithm.

## Implementation and validation

- After intersection testing, a non-leaf polygon tile that satisfies `prepared.Covers(tileGeometry)` emits its descendants without further geometry operations inside that branch.
- Descendant traversal is shared with tile-source expansion through `TileAccumulator.TryAddDescendants`. Every leaf still goes through distinct-tile counting and cancellation; failed or cancelled computations are not cached.
- Budgets below 16 skip the new coverage predicate. The threshold is a conservative heuristic (capacity for two quadtree levels), not a universally measured crossover. It uses the total budget, not remaining capacity, so overlapping components do not disable the optimization solely because earlier components already added their tiles.
- The Release test executable passed all 232 cases, including 15 new cases. New coverage checks include an independent exhaustive leaf-intersection oracle, aligned holes, repeated components, zooms 24/25, and a geometry-creation budget that detects exhaustive traversal of a fully covered 4,096-tile branch. Existing tests exercise cancellation, early rejection, cache integrity, and boundary ownership.

## Matched before/after measurements

Baseline: commit `251c71ceadaa340ea71301a24857cc95b3bdc0c9`. After: the same benchmark fixtures with this working-tree optimization. Both runs completed all 36 selected cases: four workloads, zooms 8/16/25, and `GetTiles`, complete `TryGetTiles`, and rejected `TryGetTiles`. Benchmark setup independently verifies full/bounded/minimal coverage agreement on every fixture.

BenchmarkDotNet 0.15.8, .NET 10.0.12, SDK 10.0.401, Windows x64, Intel Core i5-10400. One launch, eight warmup iterations, ten measured iterations, and a 250 ms target iteration time. Each invocation constructs a fresh cover, so the measurements include input copying but do not reuse cached results.

Run this command on each revision in separate checkouts, changing the artifacts directory for each run:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*GeometryBenchmarks.GetTiles*' '*GeometryBenchmarks.TryGetTiles*' --job short --launchCount 1 --warmupCount 8 --iterationCount 10 --iterationTime 250 --artifacts artifacts/covered-branches-final
```

Do not derive speedups by comparing this run with the original one-warmup exploratory baseline. The matching baseline was rerun with the same longer settings. Reports retain confidence intervals and allocations: [before](results/2026-10-01/covered-branches-before.md), [after](results/2026-10-01/covered-branches-after.md), and [all 36 comparisons as CSV](results/2026-10-01/covered-branches-comparison.csv). Raw CSV exports are stored alongside the reports.

### Zoom 25 full results

| Workload | Method | Before ms | After ms | Speedup | Before MiB | After MiB |
|---|---|---:|---:|---:|---:|---:|
| FullTilePolygon | GetTiles | 11.332 | 2.025 | 5.60x | 21.91 | 3.85 |
| FullTilePolygon | TryGetTilesComplete | 10.931 | 2.074 | 5.27x | 21.97 | 3.86 |
| DetailedLine | GetTiles | 370.195 | 370.407 | 1.00x | 232.75 | 232.73 |
| DetailedLine | TryGetTilesComplete | 361.309 | 372.320 | 0.97x | 232.75 | 232.75 |
| PolygonWithHole | GetTiles | 35.062 | 15.232 | 2.30x | 24.42 | 11.19 |
| PolygonWithHole | TryGetTilesComplete | 35.889 | 15.412 | 2.33x | 24.43 | 11.20 |
| MultiPolygon | GetTiles | 13.783 | 8.862 | 1.56x | 15.32 | 8.40 |
| MultiPolygon | TryGetTilesComplete | 14.159 | 8.849 | 1.60x | 15.39 | 8.38 |

Across all three zooms, polygon full-result calls improved approximately 1.6–5.6 times and allocated 45–83% less memory. Detailed-line means were roughly 0–4% higher with essentially unchanged allocations; these runs do not establish exact performance equivalence for the line path. The synthetic footprints scale with zoom and cap output at 1,024 tiles; they do not represent an expanding fixed geographic area.

### Rejected requests and JIT control

The default-tiered runs still measured some slower rejected polygon requests despite the small-budget fallback. At zoom 25, the polygon-with-hole mean changed from 0.860 ms to 1.177 ms and MultiPolygon from 0.320 ms to 0.483 ms. Those observations are retained, not treated as zero regression.

To isolate sensitivity to runtime compilation, the unchanged original commit was extracted to a temporary source snapshot and both revisions were measured again for all 12 rejection cases with `DOTNET_TieredCompilation=0`, five warmups, ten measurements, and a 250 ms target iteration time:

```shell
dotnet run --project benchmarks/GeoTileCover.Benchmarks -c Release -- --filter '*GeometryBenchmarks.TryGetTilesRejected*' --job short --launchCount 1 --warmupCount 5 --iterationCount 10 --iterationTime 250 --envVars DOTNET_TieredCompilation:0 --artifacts artifacts/covered-branches-rejected-final-jit
```

In that control, reported allocations were identical for all 12 pairs and mean changes ranged from -1.6% to +3.7%. At zoom 25, the polygon-with-hole changed from 1.217 ms to 1.222 ms, and MultiPolygon from 0.501 ms to 0.493 ms. [Before control](results/2026-10-01/rejected-original-no-tiering.md), [after control](results/2026-10-01/rejected-after-no-tiering.md), and [comparison CSV](results/2026-10-01/rejected-no-tiering-comparison.csv).

This is consistent with runtime warmup/profile effects contributing to the default-run differences; it is not proof that every default-runtime workload is regression-free. The controlled timings are a separate experiment and must not be mixed with the default-tiered full-cover speedup calculations. The 16-tile guard guarantees that tiny requests do not pay for additional `Covers` calls, not a universal latency guarantee.

## Limits of the conclusion

These are local, single-launch measurements on synthetic inputs. Some actual iterations were shorter than the target and BenchmarkDotNet removed upper outliers; consult the error columns before interpreting small differences. The measured improvements justify the covered-branch shortcut for these full-result polygon workloads. Intermediate tile budgets, very thin polygons, real-world geometry distributions, and steady-state behavior in a long-lived application can have different tradeoffs.

Full enumeration still costs at least one output tile per result. Boundary-only neighboring branches continue through ordinary geometry traversal. The change does not turn the result into a minimal representation, remove the output-size cost, or add cancellation inside individual NetTopologySuite operations.
