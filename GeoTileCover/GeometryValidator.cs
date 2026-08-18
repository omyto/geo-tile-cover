using System;
using NetTopologySuite.Geometries;

namespace GeoTileCover;

internal static class GeometryValidator
{
    public static void Validate(Geometry geometry)
    {
        if (geometry is Polygon polygon)
        {
            ValidateSequence(polygon.ExteriorRing.CoordinateSequence, checkAntimeridian: true);
            for (var index = 0; index < polygon.NumInteriorRings; index++)
            {
                ValidateSequence(polygon.GetInteriorRingN(index).CoordinateSequence, checkAntimeridian: true);
            }

            return;
        }

        if (geometry is LineString lineString)
        {
            ValidateSequence(lineString.CoordinateSequence, checkAntimeridian: true);
            return;
        }

        if (geometry is Point point)
        {
            ValidateSequence(point.CoordinateSequence, checkAntimeridian: false);
            return;
        }

        if (geometry is GeometryCollection collection)
        {
            for (var index = 0; index < collection.NumGeometries; index++)
            {
                Validate(collection.GetGeometryN(index));
            }

            return;
        }

        ValidateCoordinates(geometry.Coordinates);
    }

    private static void ValidateSequence(CoordinateSequence sequence, bool checkAntimeridian)
    {
        for (var index = 0; index < sequence.Count; index++)
        {
            ValidateCoordinate(sequence.GetX(index), sequence.GetY(index));
            if (checkAntimeridian && index > 0 && Math.Abs(sequence.GetX(index) - sequence.GetX(index - 1)) > 180d)
            {
                throw new NotSupportedException("Geometry segments crossing the antimeridian must be split before covering tiles.");
            }
        }
    }

    private static void ValidateCoordinates(Coordinate[] coordinates)
    {
        foreach (var coordinate in coordinates)
        {
            ValidateCoordinate(coordinate.X, coordinate.Y);
        }
    }

    private static void ValidateCoordinate(double longitude, double latitude)
    {
        if (double.IsNaN(longitude) || double.IsInfinity(longitude) || longitude < -180d || longitude > 180d)
        {
            throw new ArgumentOutOfRangeException("geometry", "Geometry longitude must be finite and between -180 and 180 degrees.");
        }

        if (double.IsNaN(latitude) || double.IsInfinity(latitude) || latitude < -TileMath.WebMercatorLatitudeLimit || latitude > TileMath.WebMercatorLatitudeLimit)
        {
            throw new ArgumentOutOfRangeException("geometry", $"Geometry latitude must be finite and between {-TileMath.WebMercatorLatitudeLimit} and {TileMath.WebMercatorLatitudeLimit} degrees.");
        }
    }
}

