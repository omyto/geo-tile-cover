```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6466/22H2/2022Update)
Unknown processor
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3
  Job-FJSXIK : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v3

IterationCount=7  IterationTime=100ms  LaunchCount=1  
WarmupCount=3  

```
| Type              | Method       | Pattern    | Variant      | Mean                | Error             | StdDev            | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated | Alloc Ratio |
|------------------ |------------- |----------- |------------- |--------------------:|------------------:|------------------:|------:|--------:|---------:|---------:|---------:|----------:|------------:|
| **HashBenchmarks**    | **LongXor**      | **?**          | **?**            |           **0.5269 ns** |         **0.0271 ns** |         **0.0120 ns** |  **1.00** |    **0.03** |        **-** |        **-** |        **-** |         **-** |          **NA** |
| HashBenchmarks    | MurmurLow32  | ?          | ?            |           1.1673 ns |         0.0645 ns |         0.0230 ns |  2.22 |    0.06 |        - |        - |        - |         - |          NA |
| HashBenchmarks    | MurmurFold32 | ?          | ?            |           1.3443 ns |         0.1401 ns |         0.0500 ns |  2.55 |    0.10 |        - |        - |        - |         - |          NA |
| HashBenchmarks    | XxHashLow32  | ?          | ?            |           1.1996 ns |         0.0629 ns |         0.0224 ns |  2.28 |    0.06 |        - |        - |        - |         - |          NA |
| HashBenchmarks    | XxHashFold32 | ?          | ?            |           1.3559 ns |         0.1756 ns |         0.0626 ns |  2.57 |    0.12 |        - |        - |        - |         - |          NA |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **CoverBenchmarks**   | **FromTiles**    | **Correlated** | **?**            |   **8,644,220.8333 ns** | **1,045,409.5568 ns** |   **372,803.1181 ns** |     **?** |       **?** | **500.0000** | **500.0000** | **500.0000** | **2762624 B** |           **?** |
| CoverBenchmarks   | FromPoints   | Correlated | ?            |   8,492,525.0000 ns | 2,153,866.2512 ns |   956,329.9648 ns |     ? |       ? | 500.0000 | 250.0000 | 250.0000 | 4281748 B |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **CoverBenchmarks**   | **FromTiles**    | **Grid**       | **?**            |   **6,315,087.7551 ns** | **1,048,827.1839 ns** |   **465,685.7701 ns** |     **?** |       **?** | **285.7143** | **285.7143** | **285.7143** | **2007192 B** |           **?** |
| CoverBenchmarks   | FromPoints   | Grid       | ?            |   8,270,787.5000 ns |   949,223.6033 ns |   338,502.2805 ns |     ? |       ? | 500.0000 | 250.0000 | 250.0000 | 4281748 B |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Correlated** | **LongXor**      | **413,024,542.8571 ns** | **6,129,315.2207 ns** | **2,721,453.9465 ns** |     **?** |       **?** |        **-** |        **-** |        **-** |  **420656 B** |           **?** |
| HashSetBenchmarks | Contains     | Correlated | LongXor      | 386,773,457.1429 ns | 7,649,288.2616 ns | 3,396,331.3972 ns |     ? |       ? |        - |        - |        - |      48 B |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Grid**       | **LongXor**      |     **286,358.0729 ns** |    **12,023.5968 ns** |     **4,287.7304 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Grid       | LongXor      |     147,732.1925 ns |    35,397.1071 ns |    12,622.9494 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Random**     | **LongXor**      |     **391,420.4327 ns** |    **96,348.3841 ns** |    **34,358.7619 ns** |     **?** |       **?** | **110.5769** | **110.5769** | **110.5769** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Random     | LongXor      |     258,941.8527 ns |    21,231.0536 ns |     9,426.7194 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Correlated** | **MurmurLow32**  |     **421,914.5182 ns** |    **34,603.7981 ns** |    **12,340.0477 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Correlated | MurmurLow32  |     293,495.5163 ns |    28,605.7340 ns |    10,201.0803 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Grid**       | **MurmurLow32**  |     **445,104.7619 ns** |    **90,711.2248 ns** |    **40,276.3460 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Grid       | MurmurLow32  |     291,692.8834 ns |    21,355.6629 ns |     9,482.0467 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Random**     | **MurmurLow32**  |     **471,447.8795 ns** |   **203,386.5358 ns** |    **90,304.8824 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Random     | MurmurLow32  |     306,519.7917 ns |    25,590.5060 ns |     9,125.8209 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Correlated** | **MurmurFold32** |     **435,730.7292 ns** |    **40,690.5905 ns** |    **18,066.8744 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Correlated | MurmurFold32 |     299,715.9139 ns |    10,257.9179 ns |     4,554.5791 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Grid**       | **MurmurFold32** |     **470,182.0033 ns** |    **91,273.4828 ns** |    **40,525.9920 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Grid       | MurmurFold32 |     297,785.8718 ns |    29,760.7674 ns |    13,213.9652 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Random**     | **MurmurFold32** |     **407,395.4167 ns** |    **67,332.3649 ns** |    **29,895.9873 ns** |     **?** |       **?** | **108.3333** | **108.3333** | **108.3333** |  **420612 B** |           **?** |
| HashSetBenchmarks | Contains     | Random     | MurmurFold32 |     297,918.3712 ns |    30,342.1841 ns |    10,820.3151 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **CoverBenchmarks**   | **FromTiles**    | **Random**     | **?**            |  **12,358,139.2857 ns** | **2,143,200.0729 ns** |   **951,594.1156 ns** |     **?** |       **?** | **500.0000** | **500.0000** | **500.0000** | **2762624 B** |           **?** |
| CoverBenchmarks   | FromPoints   | Random     | ?            |   9,712,171.4286 ns | 3,177,006.0570 ns | 1,410,610.3799 ns |     ? |       ? | 500.0000 | 250.0000 | 250.0000 | 4281748 B |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Correlated** | **XxHashLow32**  |     **431,682.9861 ns** |    **48,003.6363 ns** |    **17,118.5591 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Correlated | XxHashLow32  |     311,184.5063 ns |    51,650.4076 ns |    22,933.1011 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Grid**       | **XxHashLow32**  |     **412,773.7500 ns** |    **41,221.2703 ns** |    **14,699.9021 ns** |     **?** |       **?** | **108.3333** | **108.3333** | **108.3333** |  **420612 B** |           **?** |
| HashSetBenchmarks | Contains     | Grid       | XxHashLow32  |     292,118.3449 ns |    10,281.0521 ns |     3,666.3222 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Random**     | **XxHashLow32**  |     **416,850.8333 ns** |    **39,966.7256 ns** |    **17,745.4739 ns** |     **?** |       **?** | **108.3333** | **108.3333** | **108.3333** |  **420612 B** |           **?** |
| HashSetBenchmarks | Contains     | Random     | XxHashLow32  |     283,873.2008 ns |    11,757.7220 ns |     4,192.9169 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Correlated** | **XxHashFold32** |     **408,155.6920 ns** |    **31,574.6294 ns** |    **14,019.3312 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Correlated | XxHashFold32 |     298,588.7784 ns |    17,481.4664 ns |     7,761.8795 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Grid**       | **XxHashFold32** |     **405,180.2951 ns** |    **10,616.9068 ns** |     **3,786.0912 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Grid       | XxHashFold32 |     298,859.4401 ns |    23,692.7296 ns |     8,449.0556 ns |     ? |       ? |        - |        - |        - |         - |           ? |
|                   |              |            |              |                     |                   |                   |       |         |          |          |          |           |             |
| **HashSetBenchmarks** | **Build**        | **Random**     | **XxHashFold32** |     **415,759.3750 ns** |    **70,674.1930 ns** |    **25,203.0980 ns** |     **?** |       **?** | **109.3750** | **109.3750** | **109.3750** |  **420613 B** |           **?** |
| HashSetBenchmarks | Contains     | Random     | XxHashFold32 |     314,244.1702 ns |    54,408.0647 ns |    24,157.5179 ns |     ? |       ? |        - |        - |        - |         - |           ? |
