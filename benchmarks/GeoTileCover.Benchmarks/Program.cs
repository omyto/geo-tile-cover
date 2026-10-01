using BenchmarkDotNet.Running;
using GeoTileCover.Benchmarks;

if (args.Contains("--validate-geometry"))
{
    GeometryBenchmarks.ValidateWorkloads();
    return;
}

if (args.Contains("--distribution"))
{
    DistributionReport.Write();
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
