# GeoTileCover ASP.NET Core demo

This minimal API accepts a GeoJSON geometry, returns its XYZ tile cover, and keeps the `TileCover` instance in memory for reuse at other zoom levels.

Run the demo from the repository root:

```bash
dotnet run --project samples/GeoTileCover.AspNetCoreDemo
```

The default launch profile listens on `http://localhost:5000`, matching `GeoTileCover.AspNetCoreDemo.http`. It uses HTTP so no local development certificate is required.

The POST endpoint validates geometry topology before computing a cover. Invalid geometries, including self-intersecting polygons, holes outside their shell, and overlapping MultiPolygon components, return `400 Bad Request` with the reason under `errors.Geometry`. Geometry is never repaired automatically. Valid empty geometries are accepted.

Create a cover:

```http
POST /covers?includeBounds=true
Content-Type: application/json

{
  "geometry": {
    "type": "Point",
    "coordinates": [105.8342, 21.0278]
  },
  "zoom": 12
}
```

The response contains `coverId`, `expiresAt`, and the tiles for the requested zoom. Each tile contains `z`, `x`, and `y`, plus optional `bounds` when `includeBounds=true`. These coordinates uniquely identify the tile; a packed numeric tile ID is not included. Reuse the cover at another zoom with:

```http
GET /covers/{coverId}/tiles?zoom=14&includeBounds=true
```

Set the optional `includeBounds` query parameter to `true` to add `bounds` to each tile as `[west, south, east, north]`. When omitted or `false`, the `bounds` property is not included in the response.

Covers expire 15 minutes after creation, and the demo returns at most 100,000 tiles per response. A missing or expired ID returns `404 Not Found`. This in-memory approach is intended for a single-process demo; a production application needs appropriate resource limits and, when running multiple instances, a shared-storage or routing strategy.

Both endpoints use `TryGetTiles` to stop traversal when a 100,001st distinct tile is found and return `422 Unprocessable Entity`. Failed or canceled calculations do not cache partial results. Tile calculation observes request cancellation and a 10-second cooperative timeout, including time spent waiting for the cover's cache lock; a computation timeout returns `503 Service Unavailable`. The timeout does not interrupt individual NetTopologySuite operations or cover JSON parsing, topology validation, and geometry construction. Request-size, geometry-complexity, concurrency, and total-cache limits still need to be configured for a production deployment.

Run the HTTP regression checks with the demo running (PowerShell 7+):

```powershell
pwsh -File samples/GeoTileCover.AspNetCoreDemo/tests/Validate-Geometry.ps1 -BaseUrl http://localhost:5000
```

The checks cover invalid topology, valid polygons with holes, collections, empty geometry, cached GET requests, and the tile response shape.
