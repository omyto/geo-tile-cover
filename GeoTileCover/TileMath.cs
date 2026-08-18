using System;
using NetTopologySuite.Geometries;

namespace GeoTileCover;

/// <summary>Conversions between WGS84 longitude/latitude and XYZ tiles.</summary>
internal static class TileMath
{
    /// <summary>The northern and southern latitude limits of Web Mercator.</summary>
    public const double WebMercatorLatitudeLimit = 85.0511287798066;

    public static int TilesPerAxis(int zoom)
    {
        if (zoom < TileCover.MinZoom || zoom > TileCover.MaxZoom)
        {
            throw new ArgumentOutOfRangeException(nameof(zoom), $"Zoom must be between {TileCover.MinZoom} and {TileCover.MaxZoom}.");
        }

        return 1 << zoom;
    }

    public static TileId ToTile(double longitude, double latitude, int zoom)
    {
        var tilesPerAxis = TilesPerAxis(zoom);
        var boundedLongitude = Math.Max(-180d, Math.Min(180d, longitude));
        var boundedLatitude = Math.Max(-WebMercatorLatitudeLimit, Math.Min(WebMercatorLatitudeLimit, latitude));
        var x = (int)Math.Floor((boundedLongitude + 180d) / 360d * tilesPerAxis);
        var latitudeRadians = boundedLatitude * Math.PI / 180d;
        var mercatorY = Math.Log(Math.Tan(latitudeRadians) + 1d / Math.Cos(latitudeRadians)) / Math.PI;
        var y = (int)Math.Floor((1d - mercatorY) / 2d * tilesPerAxis);

        return new TileId(zoom, Math.Min(tilesPerAxis - 1, Math.Max(0, x)), Math.Min(tilesPerAxis - 1, Math.Max(0, y)));
    }

    public static Envelope ToEnvelope(TileId tile)
    {
        var tilesPerAxis = TilesPerAxis(tile.Z);
        var west = tile.X / (double)tilesPerAxis * 360d - 180d;
        var east = (tile.X + 1d) / tilesPerAxis * 360d - 180d;
        var north = TileYToLatitude(tile.Y, tilesPerAxis);
        var south = TileYToLatitude(tile.Y + 1d, tilesPerAxis);
        return new Envelope(west, east, south, north);
    }

    private static double TileYToLatitude(double y, int tilesPerAxis)
    {
        var radians = Math.Atan(Math.Sinh(Math.PI * (1d - 2d * y / tilesPerAxis)));
        return radians * 180d / Math.PI;
    }
}
