# GeoTileCover

Small .NET library for listing Web Mercator **XYZ tiles** intersecting a geometry or represented by a full-tile union.

The library targets `netstandard2.0`, so it can be consumed by .NET Framework 4.7.2+, .NET Core, and current .NET applications.

## Install

```bash
dotnet add package GeoTileCover --version 0.1.0-preview.1
```

## Usage

Input geometries use WGS84 coordinate order: **X = longitude, Y = latitude**.

Longitude must be within **-180–180°** and latitude within the Web Mercator limit of **±85.0511287798066°**. Out-of-range coordinates are rejected. Geometry segments that cross the antimeridian are also rejected and must be split before calling the library.

Supported zoom levels are exposed as `TileCover.MinZoom` and `TileCover.MaxZoom`; the current inclusive range is **0–25**.

```csharp
using GeoTileCover;
using NetTopologySuite.Geometries;

var geometry = new GeometryFactory().CreatePoint(new Coordinate(105.8342, 21.0278));
var cover = new TileCover(geometry);
foreach (var tile in cover.GetTiles(minZoom: 10, maxZoom: 12))
{
    Console.WriteLine(tile); // z/x/y, e.g. 10/813/450
    Console.WriteLine(tile.Id); // stable, collision-free 64-bit numeric ID
}
```

`TileId.Id` packs Z, X, and Y into a `long`. It can be restored with `new TileId(id)`.

`TileCover` takes a snapshot of the input geometry. Tile results are computed lazily and cached by zoom on the instance, so repeated and overlapping `GetTiles` calls reuse prior work.

A cover can also be restored from fully covered tiles, including tiles at different zoom levels:

```csharp
var restored = new TileCover(new[]
{
    new TileId(10, 813, 450),
    new TileId(12, 3257, 1802)
});

var zoom11Tiles = restored.GetTiles(11);
```

Tile inputs are treated as a union of complete tile areas. The constructor snapshots and compacts the union without expanding everything to the highest source zoom. Descendants are generated only when a higher zoom is requested, while lower zooms are derived through parent tiles.

`GetTiles` returns every tile covered by the source, ordered by zoom, then row (`y`), then column (`x`). For geometry sources, points on tile edges or corners belong to exactly one XYZ tile. A line on a vertical boundary belongs to the eastern tile, while a line on a horizontal boundary belongs to the southern tile. A polygon or line endpoint touching only a tile boundary does not include the neighboring tile. The geometry algorithm traverses the XYZ quadtree and prunes branches that do not intersect the geometry instead of enumerating every tile in its bounding box.

Use `GetMinimalTiles(minZoom, maxZoom)` to return the smallest mixed-zoom tile array that preserves the cover at `maxZoom` resolution. Complete groups of four sibling tiles are recursively replaced by their parent, stopping at `minZoom`.

```csharp
TileId[] minimalTiles = cover.GetMinimalTiles(minZoom: 10, maxZoom: 12);
```

## Layout

```text
GeoTileCover/        library and NuGet project
GeoTileCover.Tests/  unit tests
```

The repository root deliberately stays language-neutral; Java and Go implementations can be added later without renaming the repository.

## Feedback

Report bugs and propose features through [GitHub Issues](https://github.com/omyto/geo-tile-cover/issues).

## License

GeoTileCover is licensed under the [MIT License](LICENSE).
