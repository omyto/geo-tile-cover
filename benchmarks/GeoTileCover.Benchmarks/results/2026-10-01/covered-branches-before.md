```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i5-10400 CPU 2.90GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-MNNUOJ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

IterationCount=10  IterationTime=250ms  LaunchCount=1
WarmupCount=8

```
| Method              | Zoom | Workload        | Mean         | Error       | StdDev      | Gen0       | Gen1      | Allocated    |
|-------------------- |----- |---------------- |-------------:|------------:|------------:|-----------:|----------:|-------------:|
| **GetTiles**            | **8**    | **FullTilePolygon** |  **10,759.0 μs** |   **443.02 μs** |   **293.03 μs** |  **3500.0000** |  **250.0000** |  **22524.27 KB** |
| TryGetTilesComplete | 8    | FullTilePolygon |  10,785.3 μs |   505.11 μs |   300.58 μs |  3500.0000 |  250.0000 |  22474.73 KB |
| TryGetTilesRejected | 8    | FullTilePolygon |     997.9 μs |    51.16 μs |    30.45 μs |   333.3333 |         - |   2059.63 KB |
| **GetTiles**            | **8**    | **DetailedLine**    | **357,548.3 μs** | **1,529.72 μs** |   **800.07 μs** | **37000.0000** | **1000.0000** | **231921.71 KB** |
| TryGetTilesComplete | 8    | DetailedLine    | 356,972.6 μs | 2,221.88 μs | 1,469.64 μs | 37000.0000 | 1000.0000 | 231947.99 KB |
| TryGetTilesRejected | 8    | DetailedLine    |   7,335.2 μs |    72.10 μs |    47.69 μs |   770.8333 |  187.5000 |    4769.1 KB |
| **GetTiles**            | **8**    | **PolygonWithHole** |  **34,105.7 μs** |   **286.29 μs** |   **189.37 μs** |  **4000.0000** |  **500.0000** |  **24659.21 KB** |
| TryGetTilesComplete | 8    | PolygonWithHole |  34,017.3 μs |   224.35 μs |   148.39 μs |  3833.3333 |  166.6667 |  24401.37 KB |
| TryGetTilesRejected | 8    | PolygonWithHole |     783.4 μs |    18.85 μs |    12.47 μs |   142.8571 |  139.8810 |    876.44 KB |
| **GetTiles**            | **8**    | **MultiPolygon**    |  **14,078.3 μs** |   **392.06 μs** |   **259.32 μs** |  **2500.0000** |  **750.0000** |  **15886.55 KB** |
| TryGetTilesComplete | 8    | MultiPolygon    |  14,244.5 μs |   437.97 μs |   260.63 μs |  2500.0000 |  750.0000 |  15905.96 KB |
| TryGetTilesRejected | 8    | MultiPolygon    |     335.1 μs |    32.86 μs |    21.74 μs |    62.5000 |   15.6250 |    402.34 KB |
| **GetTiles**            | **16**   | **FullTilePolygon** |  **10,625.8 μs** |   **244.21 μs** |   **145.33 μs** |  **3500.0000** |  **250.0000** |  **22324.98 KB** |
| TryGetTilesComplete | 16   | FullTilePolygon |  10,595.1 μs |   191.88 μs |   114.18 μs |  3500.0000 |  250.0000 |  22312.62 KB |
| TryGetTilesRejected | 16   | FullTilePolygon |   1,044.4 μs |    20.33 μs |    12.10 μs |   333.3333 |         - |    2137.2 KB |
| **GetTiles**            | **16**   | **DetailedLine**    | **359,161.8 μs** |   **905.06 μs** |   **473.36 μs** | **38000.0000** | **1000.0000** | **238268.93 KB** |
| TryGetTilesComplete | 16   | DetailedLine    | 358,516.4 μs | 5,363.90 μs | 3,191.97 μs | 38000.0000 | 1000.0000 | 238208.04 KB |
| TryGetTilesRejected | 16   | DetailedLine    |   7,211.8 μs |    75.02 μs |    49.62 μs |   781.2500 |  187.5000 |   4799.33 KB |
| **GetTiles**            | **16**   | **PolygonWithHole** |  **35,381.9 μs** |   **303.85 μs** |   **180.82 μs** |  **4000.0000** |  **200.0000** |  **24707.66 KB** |
| TryGetTilesComplete | 16   | PolygonWithHole |  35,160.4 μs |   384.71 μs |   254.46 μs |  4000.0000 |  250.0000 |  24847.95 KB |
| TryGetTilesRejected | 16   | PolygonWithHole |     827.6 μs |    20.64 μs |    12.28 μs |   137.5000 |   75.0000 |    901.86 KB |
| **GetTiles**            | **16**   | **MultiPolygon**    |  **14,123.6 μs** |   **878.46 μs** |   **581.04 μs** |  **2500.0000** |  **750.0000** |  **15520.85 KB** |
| TryGetTilesComplete | 16   | MultiPolygon    |  13,936.5 μs |   380.78 μs |   251.86 μs |  2500.0000 |  750.0000 |   15529.2 KB |
| TryGetTilesRejected | 16   | MultiPolygon    |     335.9 μs |    42.01 μs |    25.00 μs |    62.5000 |   15.6250 |    416.96 KB |
| **GetTiles**            | **25**   | **FullTilePolygon** |  **11,332.5 μs** |   **530.17 μs** |   **350.67 μs** |  **3500.0000** |  **250.0000** |  **22431.01 KB** |
| TryGetTilesComplete | 25   | FullTilePolygon |  10,930.9 μs |   242.49 μs |   160.39 μs |  3500.0000 |  250.0000 |  22499.02 KB |
| TryGetTilesRejected | 25   | FullTilePolygon |   1,068.1 μs |    31.81 μs |    16.64 μs |   354.1667 |         - |   2224.45 KB |
| **GetTiles**            | **25**   | **DetailedLine**    | **370,194.8 μs** | **8,730.62 μs** | **5,195.45 μs** | **38000.0000** | **1000.0000** | **238335.38 KB** |
| TryGetTilesComplete | 25   | DetailedLine    | 361,308.8 μs |   697.86 μs |   415.29 μs | 38000.0000 | 1000.0000 | 238340.71 KB |
| TryGetTilesRejected | 25   | DetailedLine    |   7,270.9 μs |    51.49 μs |    34.06 μs |   770.8333 |  187.5000 |   4829.49 KB |
| **GetTiles**            | **25**   | **PolygonWithHole** |  **35,061.9 μs** |   **149.82 μs** |    **99.09 μs** |  **4000.0000** |  **166.6667** |  **25005.09 KB** |
| TryGetTilesComplete | 25   | PolygonWithHole |  35,888.8 μs |   516.38 μs |   307.29 μs |  4000.0000 |  250.0000 |  25013.17 KB |
| TryGetTilesRejected | 25   | PolygonWithHole |     860.1 μs |   191.84 μs |   100.34 μs |   150.0000 |   75.0000 |    926.66 KB |
| **GetTiles**            | **25**   | **MultiPolygon**    |  **13,783.0 μs** |   **309.84 μs** |   **204.94 μs** |  **2500.0000** |  **750.0000** |  **15687.95 KB** |
| TryGetTilesComplete | 25   | MultiPolygon    |  14,158.6 μs |   475.53 μs |   282.98 μs |  2500.0000 |  750.0000 |  15763.66 KB |
| TryGetTilesRejected | 25   | MultiPolygon    |     319.9 μs |    46.53 μs |    30.78 μs |    70.3125 |   15.6250 |    443.15 KB |
