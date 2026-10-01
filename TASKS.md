# Tasks

Remaining work reviewed against the source on 2026-10-01.

## Performance

- [x] Add geometry coverage benchmarks for LineString, Polygon, and MultiPolygon at zooms 8, 16, and 25, measuring runtime and allocations for `GetTiles`, complete/rejected `TryGetTiles`, and `GetMinimalTiles`. Synthetic footprints scale with zoom to bound full output to 1,024 tiles; detailed inputs contain roughly 2,000 vertices. See the [benchmark guide](benchmarks/GeoTileCover.Benchmarks/README.md#geometry-coverage).
- [ ] Evaluate a fully covered tile fast path for geometry-backed `GetTiles` and `TryGetTiles` against the benchmark baseline; implement only if measurements justify it. Preserve boundary ownership, deduplication, tile limits, and cancellation, and check for regressions on lines and rejected requests. `GetMinimalTiles` already short-circuits fully covered branches.

## Packaging and releases

- [ ] Generate a `.snupkg` symbol package and verify its portable PDBs and Source Link mappings. The current SDK already enables Source Link and generates a mapping to the GitHub commit; symbol packaging is still missing.
- [ ] Add CI to run Release builds and tests with Microsoft Testing Platform, build the sample and benchmark projects, and validate package artifacts. Ensure the test invocation works with the selected SDK.
- [ ] Add a NuGet release workflow using Trusted Publishing and verify the matching trust configuration on NuGet.org. No release workflow exists in the repository; the account-side configuration has not been verified.
