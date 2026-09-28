using GeoTileCover;
using GeoTileCover.AspNetCoreDemo;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO.Converters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<CoverStore>();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new GeoJsonConverterFactory()));

var app = builder.Build();

app.MapPost("/covers", (CreateCoverRequest request, CoverStore store, bool? includeBounds, CancellationToken cancellationToken) =>
{
    if (!TileResponseFactory.IsValidZoom(request.Zoom))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Zoom)] = [$"Zoom must be between {TileCover.MinZoom} and {TileCover.MaxZoom}."]
        });
    }

    if (request.Geometry is null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Geometry)] = ["A GeoJSON geometry is required."]
        });
    }

    try
    {
        var cover = new TileCover(request.Geometry);
        var tilesResult = TileResponseFactory.CreateTiles(cover, request.Zoom, includeBounds == true, cancellationToken);
        if (tilesResult.IsTooLarge)
        {
            return TileResponseFactory.TooManyTiles();
        }

        var cachedCover = store.Create(cover);
        return Results.Ok(new CoverResponse(cachedCover.Id, request.Zoom, cachedCover.ExpiresAt, tilesResult.Tiles));
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        return TileResponseFactory.ComputationTimedOut();
    }
    catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Geometry)] = [exception.Message]
        });
    }
});

app.MapGet("/covers/{coverId}/tiles", (Guid coverId, int zoom, CoverStore store, bool? includeBounds, CancellationToken cancellationToken) =>
{
    if (!TileResponseFactory.IsValidZoom(zoom))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(zoom)] = [$"Zoom must be between {TileCover.MinZoom} and {TileCover.MaxZoom}."]
        });
    }

    if (!store.TryGet(coverId, out var cachedCover))
    {
        return Results.NotFound(new ProblemDetails
        {
            Title = "Cover not found",
            Detail = "The cover ID does not exist or has expired.",
            Status = StatusCodes.Status404NotFound
        });
    }

    try
    {
        var tilesResult = TileResponseFactory.CreateTiles(cachedCover.Cover, zoom, includeBounds == true, cancellationToken);
        if (tilesResult.IsTooLarge)
        {
            return TileResponseFactory.TooManyTiles();
        }

        return Results.Ok(new CoverResponse(cachedCover.Id, zoom, cachedCover.ExpiresAt, tilesResult.Tiles));
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        return TileResponseFactory.ComputationTimedOut();
    }
});

app.Run();
