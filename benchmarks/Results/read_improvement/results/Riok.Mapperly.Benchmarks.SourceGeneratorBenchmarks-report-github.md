```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26200.6899)
AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.306
  [Host] : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3

Job=InProcess  Toolchain=InProcessEmitToolchain  

```
| Method       | Mean      | Error     | StdDev    | Gen0      | Gen1     | Allocated   |
|------------- |----------:|----------:|----------:|----------:|---------:|------------:|
| Compile      |  2.642 ms | 0.0525 ms | 0.1153 ms |  125.0000 |  15.6250 |   552.57 KB |
| LargeCompile | 85.923 ms | 1.1160 ms | 0.9319 ms | 3333.3333 | 666.6667 | 20239.85 KB |
