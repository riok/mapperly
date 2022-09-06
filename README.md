# Mapperly

[![Nuget](https://img.shields.io/nuget/v/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![Nuget](https://img.shields.io/nuget/vpre/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![GitHub](https://img.shields.io/github/license/riok/mapperly?style=flat-square)](https://github.com/riok/mapperly/blob/main/LICENSE)

Mapperly is a .NET source generator for generating object mappings. Inspired by MapStruct.

Because Mapperly creates the mapping code at build time, there is minimal overhead at runtime.
Even better, the generated code is perfectly readable, allowing you to verify the generated mapping code easily.

## Documentation

The documentation is available [here](https://riok.github.io/mapperly/docs/getting-started/installation).

## Quickstart

### Installation

Add the NuGet Package to your project:
```bash
dotnet add package Riok.Mapperly
```

### Create your first mapper

Create a mapper declaration as a partial class
and apply the `Riok.Mapperly.Abstractions.MapperAttribute` attribute.
Mapperly generates mapping method implementations for the defined mapping methods in the mapper.

```c#
// Mapper declaration
[Mapper]
public partial class DtoMapper
{
    public partial CarDto CarToCarDto(Car car);
}

// Mapper usage
var mapper = new DtoMapper();
var car = new Car { NumberOfSeats = 10, ... };
var dto = mapper.CarToCarDto(car);
dto.NumberOfSeats.Should().Be(10);
```

[Read the docs](https://riok.github.io/mapperly/docs/getting-started/installation) for any further information.

## License

Mapperly is [Apache 2.0](https://github.com/riok/mapperly/blob/main/LICENSE) licensed.
