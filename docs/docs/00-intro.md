# Introduction

[![Nuget](https://img.shields.io/nuget/v/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![Nuget](https://img.shields.io/nuget/vpre/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![GitHub](https://img.shields.io/github/license/riok/mapperly?style=flat-square)](https://github.com/riok/mapperly/blob/main/LICENSE)

Mapperly is a .NET source generator for generating object mappings. Inspired by MapStruct.
It drastically simplifies the implementation of object to object mappings.
One only needs to define the mapping methods signature. The implementation is provided by Mapperly.

Because Mapperly creates the mapping code at build time, there is minimal overhead at runtime.
Even better, the generated code is perfectly readable, allowing you to verify the generated mapping code easily.

Mapperly works by using .NET Source Generators.
Since no reflection is used at runtime, the generated code is completely trimming save and AOT friendly.

Mapperly is one of the fastest .NET object mapper out there, surpassing even the naive manual mapping approach!
The benchmark was generated with [Benchmark.netCoreMappers](https://github.com/mjebrahimi/Benchmark.netCoreMappers).

| Method        |       Mean |   Error |  StdDev |  Gen 0 | Allocated |
| ------------- | ---------: | ------: | ------: | -----: | --------: |
| AgileMapper   | 1,523.8 ns | 3.90 ns | 3.25 ns | 1.5106 |   3,160 B |
| TinyMapper    | 4,094.3 ns | 3.90 ns | 3.05 ns | 1.0300 |   2,160 B |
| ExpressMapper | 2,595.8 ns | 5.49 ns | 5.14 ns | 2.3422 |   4,904 B |
| AutoMapper    | 1,203.9 ns | 2.30 ns | 2.15 ns | 0.9098 |   1,904 B |
| ManualMapping |   529.6 ns | 0.52 ns | 0.44 ns | 0.5541 |   1,160 B |
| Mapster       |   562.1 ns | 1.14 ns | 0.89 ns | 0.9098 |   1,904 B |
| Mapperly      |   338.5 ns | 0.95 ns | 0.84 ns | 0.4396 |     920 B |

## Requirements

Mapperly supports .NET 5+ and .NET Framework 4.x.
Mapperly requires at least C# language version 8.
