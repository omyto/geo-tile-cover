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
| **TryGetTilesRejected** | **8**    | **FullTilePolygon** | **2,097.5 μs** |  **49.96 μs** | **33.04 μs** | **367.1875** |  **15.6250** | **2268.38 KB** |
| **TryGetTilesRejected** | **8**    | **DetailedLine**    | **8,473.0 μs** |  **38.76 μs** | **20.27 μs** | **781.2500** | **187.5000** | **4786.38 KB** |
| **TryGetTilesRejected** | **8**    | **PolygonWithHole** | **1,184.6 μs** |  **30.98 μs** | **20.49 μs** | **142.8571** |  **84.8214** |   **899.4 KB** |
| **TryGetTilesRejected** | **8**    | **MultiPolygon**    |   **469.7 μs** |  **14.84 μs** |  **9.82 μs** |  **68.1818** |  **18.9394** |  **421.33 KB** |
| **TryGetTilesRejected** | **16**   | **FullTilePolygon** | **2,130.6 μs** |   **6.58 μs** |  **3.92 μs** | **382.8125** |  **15.6250** | **2350.44 KB** |
| **TryGetTilesRejected** | **16**   | **DetailedLine**    | **8,640.5 μs** | **114.15 μs** | **67.93 μs** | **781.2500** | **187.5000** | **4819.04 KB** |
| **TryGetTilesRejected** | **16**   | **PolygonWithHole** | **1,185.3 μs** |  **16.06 μs** | **10.63 μs** | **147.3214** |  **75.8929** |   **922.8 KB** |
| **TryGetTilesRejected** | **16**   | **MultiPolygon**    |   **473.7 μs** |   **9.21 μs** |  **6.09 μs** |  **69.8529** |  **20.2206** |  **436.28 KB** |
| **TryGetTilesRejected** | **25**   | **FullTilePolygon** | **2,160.1 μs** |  **39.91 μs** | **26.40 μs** | **398.4375** |  **15.6250** | **2442.76 KB** |
| **TryGetTilesRejected** | **25**   | **DetailedLine**    | **8,652.1 μs** |  **99.91 μs** | **66.08 μs** | **781.2500** | **187.5000** |  **4847.3 KB** |
| **TryGetTilesRejected** | **25**   | **PolygonWithHole** | **1,216.5 μs** |  **15.11 μs** |  **9.99 μs** | **151.7857** |  **84.8214** |  **949.17 KB** |
| **TryGetTilesRejected** | **25**   | **MultiPolygon**    |   **501.0 μs** |   **4.99 μs** |  **3.30 μs** |  **74.2188** |  **19.5313** |  **461.88 KB** |
