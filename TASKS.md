# Tasks

## Minimum gate for `0.1.0-preview`

- [x] Add basic tests for Point, LineString, Polygon, MultiPolygon, and empty geometry.
- [x] Define point ownership on tile edges and corners.
- [x] Exclude neighboring tiles touched only by a polygon boundary.
- [x] Reject coordinates outside WGS84/Web Mercator bounds.
- [x] Detect and reject geometry segments crossing the antimeridian.
- [x] Define LineString ownership when it lies on a tile boundary.
- [x] Add tests at zoom levels 24 and 25.
- [x] Run release tests and inspect the rebuilt NuGet package.

## Follow-up

- [ ] Optimize branches where the geometry fully covers a tile.
- [ ] Benchmark large geometries and high zoom levels.
- [ ] Add Source Link and symbol package generation.
- [ ] Add CI and Trusted Publishing for NuGet releases.
