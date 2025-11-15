```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26200.6899)
AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.306
  [Host] : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3

Job=InProcess  Toolchain=InProcessEmitToolchain  

```
| Method       | Mean      | Error     | StdDev    | Gen0      | Gen1     | Allocated   |
|------------- |----------:|----------:|----------:|----------:|---------:|------------:|
| Compile      |  2.182 ms | 0.0543 ms | 0.1541 ms |  109.3750 |  15.6250 |   465.33 KB |
| LargeCompile | 86.925 ms | 1.6581 ms | 2.0970 ms | 3400.0000 | 800.0000 | 20238.89 KB |
