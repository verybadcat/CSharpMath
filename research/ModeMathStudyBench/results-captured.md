# Captured benchmark aggregate

This aggregate is computed from `raw-results.ndjson`: 10 fresh cold samples per library and 3 steady samples per library (each steady metric has 3 records). Values are medians; ranges are shown for elapsed time. The capture used ModeMath 32 px and CSharpMath 24 pt at 96 DPI.

| Operation | ModeMath | CSharpMath |
|---|---:|---:|
| Cold render (ms; range) | 229.681 (179.948-401.301) | 513.767 (367.005-737.039) |
| Cold managed allocation (B; median) | 267,800 | 22,053,456 |
| Cold retained (B; median) | 90,192 | 3,896,944 |
| Parse, steady per formula (us / alloc) | 10.117 (7.326-11.556) / 2.833 KB | 27.631 (22.856-36.967) / 5.703 KB |
| Layout, steady per formula (us / alloc) | 18.030 (14.278-22.914) / 6.682 KB | 69.908 (65.629-100.304) / 31.749 KB |
| Draw, steady per formula (us / alloc) | 35.735 (33.928-38.618) / 1.586 KB | 182.400 (154.539-235.238) / 10.094 KB |
| Editor, 3-key sequence (us / alloc) | 22.666 (16.616-49.191) / 6.758 KB | 39.297 (27.710-50.965) / 25.307 KB |

The normalized capture has 10 cold and 3 steady samples per library. Results are directional/manual, managed-only, and use different editor histories/timers; they are not a performance gate. Skia native allocations, JIT/code pages, process startup, and platform variance are outside these numbers.
