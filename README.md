# GeoTileCover

Find the Web Mercator **XYZ tiles** covered by a [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) geometry or a set of existing tiles. Supports points, lines, polygons, multi-geometries, and geometry collections at zooms **0–25**.

## Installation

```shell
dotnet add package GeoTileCover
```

## Quick start

Use WGS84 coordinates in **longitude, latitude** order.

```csharp
using System;
using System.Threading;
using GeoTileCover;
using NetTopologySuite.Geometries;

var geometry = new GeometryFactory().CreatePoint(new Coordinate(105.8342, 21.0278));
var cover = new TileCover(geometry);

foreach (var tile in cover.GetTiles(10))
{
    Console.WriteLine(tile); // 10/813/450
}
```

Each `TileId` has `Z` (zoom), `X` (column, increasing eastward), and `Y` (row, increasing southward). The examples below reuse `cover`.

A `TileCover` snapshots its input. Later changes to the source geometry or tile collection have no effect. Reuse the instance to benefit from cached results.

## Methods

| Method | Result |
|---|---|
| `GetTiles(zoom)` | All covered tiles at one zoom. |
| `GetTiles(minZoom, maxZoom)` | Covered tiles at every zoom in the inclusive range. |
| `TryGetTiles(zoom, maxTiles, out tiles, cancellationToken)` | Complete coverage at one zoom if it fits the limit; the cancellation token is optional. |
| `GetMinimalTiles(minZoom, maxZoom)` | The smallest mixed-zoom set preserving coverage at `maxZoom`. |

Results are deduplicated and ordered by `Z`, then `Y`, then `X`. Zoom ranges must satisfy `TileCover.MinZoom <= minZoom <= maxZoom <= TileCover.MaxZoom` (currently 0–25). Returned arrays can be modified without affecting later results.

`GetTiles` computes the complete result before returning and has no tile-count limit. LINQ `Take()` does not limit that work; use `TryGetTiles` for large or unpredictable inputs.

### Bounded results

```csharp
using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));

if (cover.TryGetTiles(12, maxTiles: 100_000, out var tiles, cancellationToken: cancellation.Token))
{
    Console.WriteLine($"Covered tiles: {tiles.Length}");
}
else
{
    Console.WriteLine("The cover exceeds 100,000 tiles.");
}
```

`true` returns the complete result; `false` returns an empty array when the limit is exceeded. `maxTiles` must be nonnegative; zero accepts only an empty cover. The limit also applies to cached results.

Cancellation throws `OperationCanceledException`. It is cooperative and cannot interrupt an individual NetTopologySuite operation, so a deadline is not a hard timeout. The tile limit does not bound geometry complexity or total computation time.

### Minimal cover

```csharp
TileId[] minimal = cover.GetMinimalTiles(minZoom: 10, maxZoom: 12);
```

Complete groups of four siblings are replaced by their parent, stopping at `minZoom`. For example, `2/0/0`, `2/1/0`, `2/0/1`, and `2/1/1` reduce to `1/0/0` if zoom 1 is allowed.

Expanding the result to `maxZoom` reproduces `GetTiles(maxZoom)`. This preserves coverage at that tile resolution; a single point remains a tile at `maxZoom`. This method has no tile-count limit or cancellation parameter.

## Existing tiles

Pass an `IEnumerable<TileId>` to cover the union of complete tile areas. Inputs may mix zoom levels, overlap, or repeat.

```csharp
var restored = new TileCover(new[] { new TileId(1, 0, 0), new TileId(2, 2, 0) });

var zoom2Tiles = restored.GetTiles(2); // Four children of 1/0/0, plus 2/2/0: five tiles.
var compactTiles = restored.GetMinimalTiles(0, 2); // 1/0/0 and 2/2/0.
```

Finer zooms include all descendants of source tiles; coarser zooms include their ancestors.

## TileId

An immutable value type with equality based on `Z`, `X`, and `Y`, suitable for dictionary keys and sets.

```csharp
var tileId = new TileId(10, 813, 450);

long id = tileId.Id;                   // Unique 64-bit ID including zoom.
int packedXY = tileId.PackedXY;        // Unique within a zoom level.
string xyz = tileId.ToString();        // "10/813/450"
TileId parent = tileId.Parent();       // 9/406/225
Envelope bounds = tileId.ToEnvelope(); // Geographic bounds in degrees.

TileId fromId = new TileId(id);        // Same tile.
```

`ToEnvelope()` returns bounds in degrees: `MinX`/`MaxX` are west/east longitudes and `MinY`/`MaxY` are south/north latitudes. `Parent()` throws `InvalidOperationException` at zoom 0.

`Id` is a stable, unique 64-bit identifier across supported zooms. Use it for storage; `GetHashCode()` is for in-memory collections. For JSON clients without exact 64-bit integer support, send `Id` as a string or send `Z`, `X`, and `Y`.

`PackedXY` stores X/Y in an `int` for zooms **0–16** (`TileId.PackedXYMaxZoom`). It is unique within a zoom level, but values can repeat across zooms. Store the pair `(z, packedXY)` and restore with `new TileId(z, packedXY)`. Negative values are valid at zoom 16. Above zoom 16, `TryGetPackedXY(out var packedXY)` returns `false`, while `PackedXY` throws `InvalidOperationException`.

## Input rules

- **Coordinates:** X = longitude, Y = latitude, in degrees. SRID must be `0` (interpreted as WGS84) or `4326`. Coordinates are not reprojected.
- **Bounds:** coordinates must be finite; longitude within **−180° to 180°**, latitude within **±85.0511287798066°**. Out-of-range coordinates are rejected.
- **Antimeridian:** split crossings first. Segments with a longitude change greater than 180° throw `NotSupportedException`.
- **Topology:** the caller supplies valid geometry. The library does not validate or repair topology; geometry-operation exceptions may propagate. Use NetTopologySuite's `IsValid` or `IsValidOp` when validation is needed.
- **Empty/null:** empty geometries and tile collections return no tiles; null inputs are rejected.

### Boundaries

- Points and lines on internal edges belong to the eastern or southern tile. A point at an internal corner belongs to the southeastern tile.
- A polygon boundary or line endpoint merely touching a neighboring tile does not include it.
- Outer world boundaries belong to the outermost tiles, including longitude `180°` and the southern latitude limit.
