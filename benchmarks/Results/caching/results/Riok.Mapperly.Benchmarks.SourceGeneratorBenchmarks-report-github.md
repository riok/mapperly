```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26200.6899)
AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.306
  [Host] : .NET 9.0.10 (9.0.10, 9.0.1025.47515), X64 RyuJIT x86-64-v3

Job=InProcess  Toolchain=InProcessEmitToolchain  

```
| Method       | Mean      | Error     | StdDev    | Gen0      | Gen1     | Allocated   |
|------------- |----------:|----------:|----------:|----------:|---------:|------------:|
| Compile      |  2.588 ms | 0.0483 ms | 0.0871 ms |  125.0000 |        - |   549.07 KB |
| LargeCompile | 87.793 ms | 1.6491 ms | 1.6935 ms | 3400.0000 | 800.0000 | 20226.62 KB |
