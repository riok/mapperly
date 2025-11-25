```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26200.6899)
AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.306
  [Host] : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3

Job=InProcess  Toolchain=InProcessEmitToolchain  

```
| Method       | Mean      | Error     | StdDev    | Gen0      | Gen1     | Allocated   |
|------------- |----------:|----------:|----------:|----------:|---------:|------------:|
| Compile      |  2.392 ms | 0.0478 ms | 0.1276 ms |  125.0000 |  31.2500 |   519.03 KB |
| LargeCompile | 93.267 ms | 1.7287 ms | 1.9214 ms | 3400.0000 | 600.0000 | 21185.55 KB |
