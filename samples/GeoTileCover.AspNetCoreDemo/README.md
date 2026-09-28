# GeoTileCover ASP.NET Core demo

This minimal API accepts a GeoJSON geometry, returns its XYZ tile cover, and keeps the `TileCover` instance in memory for reuse at other zoom levels.

Run the demo from the repository root:

```bash
dotnet run --project samples/GeoTileCover.AspNetCoreDemo
```

The default launch profile listens on `http://localhost:5000`, matching `GeoTileCover.AspNetCoreDemo.http`. It uses HTTP so no local development certificate is required.

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

The response contains `coverId`, `expiresAt`, and the tiles for the requested zoom. Reuse the cover at another zoom with:

```http
GET /covers/{coverId}/tiles?zoom=14&includeBounds=true
```

Set the optional `includeBounds` query parameter to `true` to add `bounds` to each tile as `[west, south, east, north]`. When omitted or `false`, the `bounds` property is not included in the response.

Covers expire 15 minutes after creation, and the demo returns at most 100,000 tiles per response. A missing or expired ID returns `404 Not Found`. This in-memory approach is intended for a single-process demo; a production application needs appropriate resource limits and, when running multiple instances, a shared-storage or routing strategy.
