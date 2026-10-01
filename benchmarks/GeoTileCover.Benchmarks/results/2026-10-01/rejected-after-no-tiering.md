```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i5-10400 CPU 2.90GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-ZBDHHS : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

EnvironmentVariables=DOTNET_TieredCompilation=0  IterationCount=10  IterationTime=250ms
LaunchCount=1  WarmupCount=5

```
| Method              | Zoom | Workload        | Mean       | Error     | StdDev   | Gen0     | Gen1     | Allocated  |
|-------------------- |----- |---------------- |-----------:|----------:|---------:|---------:|---------:|-----------:|
| **TryGetTilesRejected** | **8**    | **FullTilePolygon** | **2,074.8 μs** |  **56.26 μs** | **33.48 μs** | **367.1875** |  **15.6250** | **2268.38 KB** |
| **TryGetTilesRejected** | **8**    | **DetailedLine**    | **8,609.3 μs** |  **29.91 μs** | **19.79 μs** | **781.2500** | **187.5000** | **4786.38 KB** |
| **TryGetTilesRejected** | **8**    | **PolygonWithHole** | **1,182.5 μs** |  **19.86 μs** | **11.82 μs** | **142.8571** |  **84.8214** |   **899.4 KB** |
| **TryGetTilesRejected** | **8**    | **MultiPolygon**    |   **487.3 μs** |  **21.67 μs** | **14.33 μs** |  **68.0147** |  **18.3824** |  **421.33 KB** |
| **TryGetTilesRejected** | **16**   | **FullTilePolygon** | **2,155.4 μs** |  **31.60 μs** | **16.53 μs** | **382.8125** |  **15.6250** | **2350.44 KB** |
| **TryGetTilesRejected** | **16**   | **DetailedLine**    | **8,529.8 μs** |  **31.39 μs** | **18.68 μs** | **781.2500** | **187.5000** | **4819.04 KB** |
| **TryGetTilesRejected** | **16**   | **PolygonWithHole** | **1,170.3 μs** |  **17.72 μs** | **10.54 μs** | **147.3214** |  **75.8929** |   **922.8 KB** |
| **TryGetTilesRejected** | **16**   | **MultiPolygon**    |   **474.2 μs** |   **8.26 μs** |  **5.46 μs** |  **69.8529** |  **20.2206** |  **436.28 KB** |
| **TryGetTilesRejected** | **25**   | **FullTilePolygon** | **2,190.4 μs** |  **18.49 μs** | **12.23 μs** | **398.4375** |  **15.6250** | **2442.76 KB** |
| **TryGetTilesRejected** | **25**   | **DetailedLine**    | **8,579.9 μs** | **106.37 μs** | **70.36 μs** | **781.2500** | **187.5000** |  **4847.3 KB** |
| **TryGetTilesRejected** | **25**   | **PolygonWithHole** | **1,221.6 μs** |  **26.07 μs** | **17.24 μs** | **153.8462** |  **81.7308** |  **949.17 KB** |
| **TryGetTilesRejected** | **25**   | **MultiPolygon**    |   **492.8 μs** |   **8.24 μs** |  **5.45 μs** |  **74.2188** |  **19.5313** |  **461.88 KB** |
