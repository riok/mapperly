```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26200.6899)
AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.306
  [Host] : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3

Job=InProcess  Toolchain=InProcessEmitToolchain  

```
| Method       | Mean      | Error     | StdDev    | Gen0      | Gen1     | Allocated   |
|------------- |----------:|----------:|----------:|----------:|---------:|------------:|
| Compile      |  2.567 ms | 0.0497 ms | 0.1247 ms |  125.0000 |  31.2500 |   554.44 KB |
| LargeCompile | 82.853 ms | 1.5447 ms | 1.4449 ms | 3166.6667 | 666.6667 | 19992.74 KB |
