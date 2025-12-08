---
sidebar_position: 0
description: Introduction into Mapperly.
---

# Introduction

[![Nuget](https://img.shields.io/nuget/v/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![Nuget](https://img.shields.io/nuget/vpre/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![License](https://img.shields.io/github/license/riok/mapperly?style=flat-square)](https://github.com/riok/mapperly/blob/main/LICENSE)
![GitHub Sponsors](https://img.shields.io/github/sponsors/riok)
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/riok/mapperly)

## What is Mapperly?

Mapperly is a .NET source generator for generating object mappings.
It drastically simplifies the implementation of object to object mappings.
One only needs to define the mapping methods signature. The implementation is provided by Mapperly.

Because Mapperly creates the mapping code at build time, there is minimal overhead at runtime.
Even better, the generated code is perfectly readable, allowing you to verify the generated mapping code easily.

Mapperly works by using .NET Source Generators.
Since no reflection is used at runtime, the generated code is completely trimming safe and AoT friendly.

Mapperly was originally inspired by [MapStruct](https://mapstruct.org/).

## Why object mappings?

In multi-layered applications, one layer often exposes different information than another.
Therefore object-to-object mappings are required between the layers.
Writing such boilerplate code by hand is tedious and error-prone.
Mapperly automates and simplifies object-to-object mappings while preserving many benefits of mappings written by hand.
Mapperly can even help you avoid bugs by providing helpful hints during the compilation process.
For example, Mapperly can report a warning when there is an added property in a class which is not yet marked as ignored but neither mapped to the target class.

## Why Mapperly?

- Mapperly does not use reflection
- Mapperly is trimming and AoT safe
- Mapperly runs at build time
- Mapperly does not have a runtime dependency
- The generated mappings are amazingly fast with minimal memory overhead
- The generated mapping code is readable and debuggable
- No need to write and maintain boilerplate code by hand
- Mapperly is pluggable: it is always possible to implement mappings for certain types by hand, which get picked up by Mapperly

## Support

Need help with Mapperly or looking for consulting services? The riok team is here to assist!
Whether you need guidance, architecture consultation, or custom feature development, feel free to reach out:

- **GitHub Discussions**: For questions and community support, visit [GitHub Discussions](https://github.com/riok/mapperly/discussions)
- **Consulting & Support**: For professional assistance, consulting, or enterprise support, contact the riok team at [hello@riok.ch](mailto:hello@riok.ch)
- **Sponsorship**: Support the project and get priority support through [GitHub Sponsors](https://github.com/sponsors/riok)

## Performance

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
