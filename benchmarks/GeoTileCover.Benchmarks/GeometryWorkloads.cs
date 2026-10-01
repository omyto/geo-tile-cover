using NetTopologySuite.Geometries;

namespace GeoTileCover.Benchmarks;

public enum GeometryWorkload
{
    FullTilePolygon,
    DetailedLine,
    PolygonWithHole,
    MultiPolygon
}

internal static class GeometryWorkloads
{
    public const int TileBudget = 1_024;
    public const int RejectionBudget = 8;
    public static readonly int[] Zooms = { 8, 16, 25 };

    public static Geometry Create(GeometryWorkload workload, int zoom)
    {
        // A parent five levels up bounds every workload to 32 x 32 target tiles.
        // Rescale the footprint, not the vertex count, when changing zoom.
        var parentZoom = zoom - 5;
        var bounds = new TileId(parentZoom, 1 << (parentZoom - 1), 1 << (parentZoom - 2)).ToEnvelope();
        var factory = new GeometryFactory(new PrecisionModel(), 4326);

        Coordinate Map(double x, double y) => new Coordinate(bounds.MinX + x * bounds.Width, bounds.MinY + y * bounds.Height);

        LinearRing Ring(double x, double y, double radius, int vertices, bool clockwise = false)
        {
            var coordinates = new Coordinate[vertices + 1];
            for (var index = 0; index < vertices; index++)
            {
                var angle = 2 * Math.PI * index / vertices * (clockwise ? -1 : 1);
                coordinates[index] = Map(x + radius * Math.Cos(angle), y + radius * Math.Sin(angle));
            }

            coordinates[vertices] = coordinates[0].Copy();
            return factory.CreateLinearRing(coordinates);
        }

        switch (workload)
        {
            case GeometryWorkload.FullTilePolygon:
                return factory.ToGeometry(bounds);
            case GeometryWorkload.DetailedLine:
                var points = new Coordinate[2_049];
                for (var index = 0; index < points.Length; index++)
                {
                    var fraction = (double)index / (points.Length - 1);
                    points[index] = Map(0.02 + 0.96 * fraction, 0.5 + 0.45 * Math.Sin(16 * Math.PI * fraction));
                }

                return factory.CreateLineString(points);
            case GeometryWorkload.PolygonWithHole:
                return factory.CreatePolygon(Ring(0.5, 0.5, 0.48, 2_048), new[] { Ring(0.5, 0.5, 0.2, 512, clockwise: true) });
            case GeometryWorkload.MultiPolygon:
                var polygons = new List<Polygon>();
                foreach (var x in new[] { 0.25, 0.75 })
                {
                    foreach (var y in new[] { 0.25, 0.75 })
                    {
                        polygons.Add(factory.CreatePolygon(Ring(x, y, 0.22, 512)));
                    }
                }

                return factory.CreateMultiPolygon(polygons.ToArray());
            default:
                throw new ArgumentOutOfRangeException(nameof(workload));
        }
    }
}
