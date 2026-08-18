# GeoTileCover

Small .NET library for listing Web Mercator **XYZ tiles** intersecting a geometry.

The library targets `netstandard2.0`, so it can be consumed by .NET Framework 4.7.2+, .NET Core, and current .NET applications.

## Install

```bash
dotnet add package GeoTileCover --version 0.1.0-preview.1
```

## Usage

Input geometries use WGS84 coordinate order: **X = longitude, Y = latitude**.

Supported zoom levels are exposed as `TileCover.MinZoom` and `TileCover.MaxZoom`; the current inclusive range is **0–25**.

```csharp
using GeoTileCover;
using NetTopologySuite.Geometries;

var geometry = new GeometryFactory().CreatePoint(new Coordinate(105.8342, 21.0278));
foreach (var tile in TileCover.GetTiles(geometry, minZoom: 10, maxZoom: 12))
{
    Console.WriteLine(tile); // z/x/y, e.g. 10/813/450
    Console.WriteLine(tile.Id); // stable, collision-free 64-bit numeric ID
}
```

`TileId.Id` packs Z, X, and Y into a `long`. It can be restored with `new TileId(id)`.

`GetTiles` returns every tile covered by the input geometry, ordered by zoom, then row (`y`), then column (`x`). Points on tile edges or corners belong to exactly one XYZ tile. For non-point geometries, touching only a tile boundary does not include the neighboring tile. The algorithm traverses the XYZ quadtree and prunes branches that do not intersect the geometry instead of enumerating every tile in its bounding box. Coordinates are interpreted as WGS84 degrees and projected according to the standard Web Mercator tile scheme. Split geometries that cross the antimeridian before covering them.

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
