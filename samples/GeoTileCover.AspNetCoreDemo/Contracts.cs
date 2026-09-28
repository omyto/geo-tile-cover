using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace GeoTileCover.AspNetCoreDemo;

internal sealed record CreateCoverRequest(Geometry? Geometry, int Zoom);

internal sealed record CoverResponse(Guid CoverId, int Zoom, DateTimeOffset ExpiresAt, TileResponse[] Tiles);

internal sealed record TileResponse(long Id, int Z, int X, int Y, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double[]? Bounds);

internal readonly record struct TilesResult(TileResponse[] Tiles, bool IsTooLarge);

internal static class TileResponseFactory
{
    public static bool IsValidZoom(int zoom) => zoom >= TileCover.MinZoom && zoom <= TileCover.MaxZoom;

    public static TilesResult CreateTiles(TileCover cover, int zoom, bool includeBounds)
    {
        var tiles = cover.GetTiles(zoom).Take(CoverOptions.MaxTilesPerResponse + 1).ToArray();
        if (tiles.Length > CoverOptions.MaxTilesPerResponse)
        {
            return new TilesResult([], true);
        }

        return new TilesResult(tiles.Select(tile => CreateTile(tile, includeBounds)).ToArray(), false);
    }

    private static TileResponse CreateTile(TileId tile, bool includeBounds)
    {
        if (!includeBounds)
        {
            return new TileResponse(tile.Id, tile.Z, tile.X, tile.Y, null);
        }

        var envelope = tile.ToEnvelope();
        return new TileResponse(tile.Id, tile.Z, tile.X, tile.Y, [envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY]);
    }

    public static IResult TooManyTiles() => Results.Problem(
        title: "Tile cover is too large",
        detail: $"The requested zoom produces more than {CoverOptions.MaxTilesPerResponse:N0} tiles.",
        statusCode: StatusCodes.Status422UnprocessableEntity);
}
