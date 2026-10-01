```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i5-10400 CPU 2.90GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-QPBZFU : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

IterationCount=3  IterationTime=100ms  LaunchCount=1
WarmupCount=1

```
| Method              | Zoom | Workload        | Mean         | Error         | StdDev       | Median       | Gen0       | Gen1      | Allocated    |
|-------------------- |----- |---------------- |-------------:|--------------:|-------------:|-------------:|-----------:|----------:|-------------:|
| **GetTiles**            | **8**    | **FullTilePolygon** |  **67,053.8 μs** |  **61,913.23 μs** |  **3,393.67 μs** |  **65,868.3 μs** |  **3000.0000** |         **-** |  **23476.71 KB** |
| TryGetTilesComplete | 8    | FullTilePolygon |  36,514.3 μs |  69,016.32 μs |  3,783.02 μs |  38,435.3 μs |  3500.0000 |         - |  22825.06 KB |
| TryGetTilesRejected | 8    | FullTilePolygon |   6,040.9 μs |   4,876.86 μs |    267.32 μs |   5,981.2 μs |   312.5000 |         - |      2173 KB |
| GetMinimalTiles     | 8    | FullTilePolygon |     638.1 μs |     436.87 μs |     23.95 μs |     649.7 μs |    34.0909 |         - |    217.55 KB |
| **GetTiles**            | **8**    | **DetailedLine**    | **365,309.7 μs** |  **28,702.73 μs** |  **1,573.29 μs** | **366,108.4 μs** | **37000.0000** | **1000.0000** |  **231921.8 KB** |
| TryGetTilesComplete | 8    | DetailedLine    | 368,408.8 μs |  80,520.98 μs |  4,413.63 μs | 369,317.8 μs | 37000.0000 | 1000.0000 | 231926.99 KB |
| TryGetTilesRejected | 8    | DetailedLine    |   7,480.7 μs |   1,761.46 μs |     96.55 μs |   7,531.0 μs |   750.0000 |  166.6667 |   4768.81 KB |
| GetMinimalTiles     | 8    | DetailedLine    | 569,469.9 μs |  58,212.29 μs |  3,190.81 μs | 567,646.8 μs | 59000.0000 | 1000.0000 | 365867.09 KB |
| **GetTiles**            | **8**    | **PolygonWithHole** | **111,941.0 μs** | **506,176.04 μs** | **27,745.22 μs** | **127,931.1 μs** |  **4000.0000** | **1000.0000** |  **24767.56 KB** |
| TryGetTilesComplete | 8    | PolygonWithHole | 115,314.7 μs | 635,101.71 μs | 34,812.07 μs | 132,667.0 μs |  4000.0000 | 1000.0000 |   24764.3 KB |
| TryGetTilesRejected | 8    | PolygonWithHole |   2,836.3 μs |   7,595.86 μs |    416.35 μs |   2,982.1 μs |   125.0000 |   93.7500 |    883.39 KB |
| GetMinimalTiles     | 8    | PolygonWithHole |  69,280.0 μs |  97,833.28 μs |  5,362.57 μs |  72,276.1 μs |  2000.0000 | 1000.0000 |  13517.98 KB |
| **GetTiles**            | **8**    | **MultiPolygon**    | **108,243.0 μs** | **701,578.91 μs** | **38,455.91 μs** | **100,646.5 μs** |  **2000.0000** |         **-** |  **16621.49 KB** |
| TryGetTilesComplete | 8    | MultiPolygon    | 120,669.0 μs | 558,611.96 μs | 30,619.41 μs | 130,506.7 μs |  2000.0000 |         - |  16396.28 KB |
| TryGetTilesRejected | 8    | MultiPolygon    |     465.8 μs |      91.35 μs |      5.01 μs |     467.2 μs |    62.5000 |   15.6250 |    401.38 KB |
| GetMinimalTiles     | 8    | MultiPolygon    |  38,401.9 μs |  62,091.13 μs |  3,403.42 μs |  40,303.8 μs |  1500.0000 |  500.0000 |  11163.06 KB |
| **GetTiles**            | **16**   | **FullTilePolygon** |  **47,551.4 μs** | **330,578.72 μs** | **18,120.14 μs** |  **46,926.4 μs** |  **3500.0000** |         **-** |  **22742.15 KB** |
| TryGetTilesComplete | 16   | FullTilePolygon |  36,343.8 μs |  50,172.16 μs |  2,750.11 μs |  37,551.4 μs |  3500.0000 |         - |  22922.31 KB |
| TryGetTilesRejected | 16   | FullTilePolygon |   5,427.8 μs |  38,705.99 μs |  2,121.61 μs |   6,399.6 μs |   312.5000 |         - |   2211.31 KB |
| GetMinimalTiles     | 16   | FullTilePolygon |   1,151.4 μs |   6,803.10 μs |    372.90 μs |   1,352.7 μs |    62.5000 |         - |    453.41 KB |
| **GetTiles**            | **16**   | **DetailedLine**    | **356,018.8 μs** |  **30,219.55 μs** |  **1,656.44 μs** | **355,317.9 μs** | **38000.0000** | **1000.0000** | **238269.02 KB** |
| TryGetTilesComplete | 16   | DetailedLine    | 368,500.7 μs |  71,398.02 μs |  3,913.57 μs | 368,487.5 μs | 38000.0000 | 1000.0000 | 238186.48 KB |
| TryGetTilesRejected | 16   | DetailedLine    |   7,689.1 μs |   8,286.40 μs |    454.21 μs |   7,451.9 μs |   769.2308 |  153.8462 |   4801.24 KB |
| GetMinimalTiles     | 16   | DetailedLine    | 595,042.0 μs | 188,557.33 μs | 10,335.46 μs | 598,498.6 μs | 62000.0000 | 1000.0000 | 380320.59 KB |
| **GetTiles**            | **16**   | **PolygonWithHole** | **107,172.3 μs** | **673,441.37 μs** | **36,913.59 μs** | **125,541.5 μs** |  **4000.0000** | **1000.0000** |  **25004.84 KB** |
| TryGetTilesComplete | 16   | PolygonWithHole | 105,819.4 μs | 813,823.79 μs | 44,608.43 μs | 130,820.7 μs |  4000.0000 | 1000.0000 |  25032.55 KB |
| TryGetTilesRejected | 16   | PolygonWithHole |   1,717.5 μs |   2,243.75 μs |    122.99 μs |   1,779.1 μs |   140.6250 |   62.5000 |    904.76 KB |
| GetMinimalTiles     | 16   | PolygonWithHole |  68,476.7 μs |  27,227.29 μs |  1,492.42 μs |  69,124.5 μs |  2000.0000 | 1000.0000 |  13779.62 KB |
| **GetTiles**            | **16**   | **MultiPolygon**    | **118,660.5 μs** | **801,974.27 μs** | **43,958.92 μs** | **131,379.1 μs** |  **2000.0000** |         **-** |   **16405.2 KB** |
| TryGetTilesComplete | 16   | MultiPolygon    | 110,175.1 μs | 685,140.47 μs | 37,554.86 μs | 104,339.0 μs |  2000.0000 |         - |  16871.05 KB |
| TryGetTilesRejected | 16   | MultiPolygon    |   1,926.5 μs |   7,752.65 μs |    424.95 μs |   2,085.3 μs |    62.5000 |   20.8333 |    431.09 KB |
| GetMinimalTiles     | 16   | MultiPolygon    |  74,110.9 μs | 101,684.02 μs |  5,573.64 μs |  70,992.9 μs |  2000.0000 |         - |  12587.84 KB |
| **GetTiles**            | **25**   | **FullTilePolygon** |  **46,817.2 μs** | **171,834.90 μs** |  **9,418.85 μs** |  **51,967.1 μs** |  **3500.0000** |         **-** |   **23019.8 KB** |
| TryGetTilesComplete | 25   | FullTilePolygon |  47,421.4 μs | 262,626.74 μs | 14,395.46 μs |  54,359.8 μs |  3500.0000 |         - |  22863.47 KB |
| TryGetTilesRejected | 25   | FullTilePolygon |   5,236.7 μs |  30,668.21 μs |  1,681.03 μs |   6,180.2 μs |   312.5000 |         - |   2278.01 KB |
| GetMinimalTiles     | 25   | FullTilePolygon |   1,853.1 μs |  10,896.97 μs |    597.30 μs |   2,158.9 μs |   104.1667 |         - |    740.59 KB |
| **GetTiles**            | **25**   | **DetailedLine**    | **364,758.3 μs** |  **14,597.73 μs** |    **800.15 μs** | **364,922.3 μs** | **38000.0000** | **1000.0000** | **238313.82 KB** |
| TryGetTilesComplete | 25   | DetailedLine    | 365,871.0 μs | 165,191.46 μs |  9,054.70 μs | 362,487.8 μs | 38000.0000 | 1000.0000 |  238340.8 KB |
| TryGetTilesRejected | 25   | DetailedLine    |   7,460.3 μs |     598.39 μs |     32.80 μs |   7,468.9 μs |   769.2308 |  153.8462 |   4829.72 KB |
| GetMinimalTiles     | 25   | DetailedLine    | 594,627.8 μs |  11,277.91 μs |    618.18 μs | 594,573.7 μs | 62000.0000 | 1000.0000 | 385141.97 KB |
| **GetTiles**            | **25**   | **PolygonWithHole** | **108,175.4 μs** | **752,883.93 μs** | **41,268.11 μs** | **129,549.1 μs** |  **4000.0000** | **1000.0000** |  **25176.41 KB** |
| TryGetTilesComplete | 25   | PolygonWithHole | 109,410.2 μs | 801,926.05 μs | 43,956.27 μs | 127,566.3 μs |  4000.0000 | 1000.0000 |  25148.87 KB |
| TryGetTilesRejected | 25   | PolygonWithHole |   2,904.0 μs |   7,897.33 μs |    432.88 μs |   3,115.2 μs |   125.0000 |   62.5000 |    932.51 KB |
| GetMinimalTiles     | 25   | PolygonWithHole |  69,836.7 μs |  34,698.45 μs |  1,901.94 μs |  70,814.1 μs |  2000.0000 | 1000.0000 |  14050.09 KB |
| **GetTiles**            | **25**   | **MultiPolygon**    | **111,000.9 μs** | **817,494.06 μs** | **44,809.61 μs** | **104,336.8 μs** |  **2000.0000** |         **-** |  **16509.26 KB** |
| TryGetTilesComplete | 25   | MultiPolygon    | 106,799.1 μs | 771,431.55 μs | 42,284.77 μs |  98,513.6 μs |  2000.0000 |         - |  16585.11 KB |
| TryGetTilesRejected | 25   | MultiPolygon    |   1,942.6 μs |   9,850.97 μs |    539.96 μs |   2,153.0 μs |    62.5000 |   20.8333 |    463.15 KB |
| GetMinimalTiles     | 25   | MultiPolygon    |  79,944.4 μs |  95,897.15 μs |  5,256.45 μs |  77,357.6 μs |  2000.0000 |         - |  13479.89 KB |
