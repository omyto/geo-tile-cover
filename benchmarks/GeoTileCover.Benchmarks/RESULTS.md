# Hash comparison, 2026-09-29

Keep the current MurmurHash3 `fmix64` followed by taking the low 32 bits. The measurements do not establish a meaningful collection-performance advantage for changing to XOR folding or classic xxHash64 avalanche. This decision is specific to the packed tile IDs tested here, not a claim that one complete hashing algorithm is universally better.

BenchmarkDotNet 0.15.8, .NET 10.0.12, Windows x64, SDK 10.0.401. One launch, three warmups, seven measured iterations, 100 ms target iteration time. All 41 benchmarks completed. The runner could not identify the CPU model or switch the power plan; these are local short-run observations, not portable performance guarantees. See the [full report](results/2026-09-29/benchmark-report.md) for confidence intervals, variability, and allocations.

## Hash-only throughput

Each invocation hashes 16,384 precomputed IDs. These are amortized throughput values, not isolated call latency.

| Method | Mean ns/hash | Reported error ns |
|---|---:|---:|
| Previous long XOR | 0.527 | 0.027 |
| Murmur, low 32 | 1.167 | 0.065 |
| Murmur, XOR halves | 1.344 | 0.140 |
| xxHash64 avalanche, low 32 | 1.200 | 0.063 |
| xxHash64 avalanche, XOR halves | 1.356 | 0.176 |

The two low-32 finalizers are close. Folding adds no measured benefit here. The full xxHash byte hashing algorithm is not being measured.

## HashSet construction

Means in milliseconds per set of 16,384 distinct tiles; all candidates use equivalent custom comparers. Most candidate-to-candidate confidence intervals overlap, so do not infer a universal ranking from the small differences.

| Method | Correlated `(128*i, i)` | Grid | Random |
|---|---:|---:|---:|
| Previous long XOR | 413.025 | 0.286 | 0.391 |
| Murmur, low 32 | 0.422 | 0.445 | 0.471 |
| Murmur, XOR halves | 0.436 | 0.470 | 0.407 |
| xxHash64 avalanche, low 32 | 0.432 | 0.413 | 0.417 |
| xxHash64 avalanche, XOR halves | 0.408 | 0.405 | 0.416 |

The old hash can be cheaper on benign data. Mixing trades a small hashing cost for avoiding catastrophic clustering on the correlated sequence. Successful lookups over the correlated set took about 386.8 ms with the old hash and 0.293 ms with Murmur low-32.

## Distribution

Each dataset contains 65,536 distinct tiles. [Raw results](results/2026-09-29/distribution.csv) cover five structured patterns and four random seeds.

- The previous long hash produces just one hash for the correlated sequence and 32,768 hashes for the 256-by-256 grid.
- All four mixed candidates have at most two extra keys sharing a hash in any sampled dataset, and no hash occurs more than twice.
- Murmur-fold happens to have no collisions in these samples. This alone does not prove it distributes better: some collisions are normal in 32 bits, and structured samples are not independent random experiments.

Both reductions have only 32 output bits. For xxHash64 avalanche specifically, folding the final result cancels the final `h ^= h >> 32` step in its low 32 bits; it should not be assumed to improve the finalizer.

## Actual library baseline

The end-to-end measurements use only the production Murmur low-32 hash; candidate comparers are not injected into the library. Constructing a cover from 16,384 tiles takes approximately 8.64 ms (correlated), 6.32 ms (grid), and 12.36 ms (random). Creating a fresh multipoint cover and calling `TryGetTiles` takes approximately 8.49, 8.27, and 9.71 ms, respectively. These establish a baseline for subsequent changes rather than comparing the four finalizers end to end.
