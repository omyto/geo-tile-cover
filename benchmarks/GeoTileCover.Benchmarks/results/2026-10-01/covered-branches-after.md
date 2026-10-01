```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i5-10400 CPU 2.90GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-MNNUOJ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

IterationCount=10  IterationTime=250ms  LaunchCount=1
WarmupCount=8

```
| Method              | Zoom | Workload        | Mean         | Error        | StdDev      | Gen0       | Gen1      | Allocated    |
|-------------------- |----- |---------------- |-------------:|-------------:|------------:|-----------:|----------:|-------------:|
| **GetTiles**            | **8**    | **FullTilePolygon** |   **1,934.4 μs** |     **44.35 μs** |    **23.20 μs** |   **593.7500** |   **93.7500** |   **3739.32 KB** |
| TryGetTilesComplete | 8    | FullTilePolygon |   1,915.9 μs |     37.37 μs |    22.24 μs |   611.1111 |  125.0000 |   3756.26 KB |
| TryGetTilesRejected | 8    | FullTilePolygon |   1,070.6 μs |     22.80 μs |    15.08 μs |   333.3333 |         - |   2059.04 KB |
| **GetTiles**            | **8**    | **DetailedLine**    | **371,239.0 μs** | **11,485.66 μs** | **7,597.05 μs** | **37000.0000** | **1000.0000** |  **231942.8 KB** |
| TryGetTilesComplete | 8    | DetailedLine    | 361,722.0 μs |  1,502.33 μs |   894.01 μs | 37000.0000 | 1000.0000 |  231926.9 KB |
| TryGetTilesRejected | 8    | DetailedLine    |   7,388.7 μs |    196.52 μs |   102.78 μs |   770.8333 |  187.5000 |   4766.99 KB |
| **GetTiles**            | **8**    | **PolygonWithHole** |  **14,734.2 μs** |    **398.10 μs** |   **263.32 μs** |  **1750.0000** |  **500.0000** |  **10742.69 KB** |
| TryGetTilesComplete | 8    | PolygonWithHole |  14,171.8 μs |    291.06 μs |   192.52 μs |  1500.0000 |  500.0000 |  10708.02 KB |
| TryGetTilesRejected | 8    | PolygonWithHole |     950.7 μs |    122.58 μs |    72.94 μs |   125.0000 |  104.1667 |    876.68 KB |
| **GetTiles**            | **8**    | **MultiPolygon**    |   **7,270.2 μs** |    **559.81 μs** |   **370.28 μs** |  **1000.0000** |  **250.0000** |   **7429.88 KB** |
| TryGetTilesComplete | 8    | MultiPolygon    |   7,094.4 μs |    327.02 μs |   216.31 μs |  1000.0000 |  250.0000 |   7475.17 KB |
| TryGetTilesRejected | 8    | MultiPolygon    |     277.4 μs |      1.99 μs |     1.32 μs |    64.6930 |   17.5439 |    401.23 KB |
| **GetTiles**            | **16**   | **FullTilePolygon** |   **1,978.2 μs** |     **95.75 μs** |    **50.08 μs** |   **593.7500** |   **93.7500** |   **3806.57 KB** |
| TryGetTilesComplete | 16   | FullTilePolygon |   1,981.6 μs |     42.23 μs |    27.93 μs |   625.0000 |   93.7500 |   3840.66 KB |
| TryGetTilesRejected | 16   | FullTilePolygon |   1,087.5 μs |     25.65 μs |    16.97 μs |   333.3333 |         - |   2147.29 KB |
| **GetTiles**            | **16**   | **DetailedLine**    | **364,575.3 μs** |  **4,836.19 μs** | **2,877.94 μs** | **38000.0000** | **1000.0000** | **238181.05 KB** |
| TryGetTilesComplete | 16   | DetailedLine    | 361,857.2 μs |  3,728.73 μs | 2,466.32 μs | 38000.0000 | 1000.0000 | 238186.38 KB |
| TryGetTilesRejected | 16   | DetailedLine    |   7,320.5 μs |     49.70 μs |    32.88 μs |   770.8333 |  187.5000 |   4801.02 KB |
| **GetTiles**            | **16**   | **PolygonWithHole** |  **15,068.4 μs** |    **362.56 μs** |   **215.76 μs** |  **1750.0000** |  **500.0000** |  **11239.53 KB** |
| TryGetTilesComplete | 16   | PolygonWithHole |  15,273.4 μs |    260.49 μs |   172.30 μs |  1750.0000 |  500.0000 |  11255.18 KB |
| TryGetTilesRejected | 16   | PolygonWithHole |   1,035.5 μs |     23.53 μs |    15.56 μs |   145.8333 |   83.3333 |     899.6 KB |
| **GetTiles**            | **16**   | **MultiPolygon**    |   **7,800.1 μs** |    **131.90 μs** |    **87.24 μs** |  **1250.0000** |  **250.0000** |   **8404.88 KB** |
| TryGetTilesComplete | 16   | MultiPolygon    |   8,532.3 μs |    742.32 μs |   491.00 μs |  1250.0000 |  250.0000 |   8444.73 KB |
| TryGetTilesRejected | 16   | MultiPolygon    |     352.8 μs |    113.30 μs |    74.94 μs |    62.5000 |   15.6250 |    416.84 KB |
| **GetTiles**            | **25**   | **FullTilePolygon** |   **2,025.2 μs** |     **54.75 μs** |    **28.64 μs** |   **625.0000** |  **125.0000** |   **3942.79 KB** |
| TryGetTilesComplete | 25   | FullTilePolygon |   2,074.4 μs |     69.14 μs |    45.73 μs |   625.0000 |  125.0000 |   3948.38 KB |
| TryGetTilesRejected | 25   | FullTilePolygon |   1,120.1 μs |     40.31 μs |    26.66 μs |   354.1667 |         - |   2207.73 KB |
| **GetTiles**            | **25**   | **DetailedLine**    | **370,407.0 μs** |  **4,675.70 μs** | **2,782.43 μs** | **38000.0000** | **1000.0000** | **238313.73 KB** |
| TryGetTilesComplete | 25   | DetailedLine    | 372,319.5 μs |  7,156.72 μs | 4,258.85 μs | 38000.0000 | 1000.0000 | 238340.71 KB |
| TryGetTilesRejected | 25   | DetailedLine    |   7,275.7 μs |     34.28 μs |    17.93 μs |   770.8333 |  187.5000 |    4827.8 KB |
| **GetTiles**            | **25**   | **PolygonWithHole** |  **15,232.0 μs** |    **289.18 μs** |   **191.28 μs** |  **1750.0000** |  **500.0000** |  **11460.59 KB** |
| TryGetTilesComplete | 25   | PolygonWithHole |  15,412.2 μs |    163.49 μs |   108.14 μs |  1750.0000 |  500.0000 |  11468.53 KB |
| TryGetTilesRejected | 25   | PolygonWithHole |   1,177.1 μs |      9.24 μs |     5.50 μs |   145.8333 |   62.5000 |    925.62 KB |
| **GetTiles**            | **25**   | **MultiPolygon**    |   **8,862.3 μs** |    **676.54 μs** |   **447.49 μs** |  **1250.0000** |  **250.0000** |    **8596.6 KB** |
| TryGetTilesComplete | 25   | MultiPolygon    |   8,848.7 μs |    906.76 μs |   599.76 μs |  1250.0000 |  250.0000 |   8583.95 KB |
| TryGetTilesRejected | 25   | MultiPolygon    |     482.9 μs |      5.12 μs |     3.05 μs |    71.4286 |   17.8571 |    441.86 KB |
