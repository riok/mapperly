# Mapperly

[![Nuget](https://img.shields.io/nuget/v/Riok.Mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![Nuget Preview](https://img.shields.io/nuget/vpre/Riok.Mapperly?style=flat-square&label=nuget%20preview)](https://www.nuget.org/packages/Riok.Mapperly/)
[![License](https://img.shields.io/github/license/riok/mapperly?style=flat-square)](https://github.com/riok/mapperly/blob/main/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/riok.mapperly?style=flat-square)](https://www.nuget.org/packages/Riok.Mapperly/)
[![GitHub Sponsors](https://img.shields.io/github/sponsors/riok)](https://github.com/sponsors/riok)
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/riok/mapperly)


Mapperly is a .NET source generator for generating object mappings.

Because Mapperly creates the mapping code at build time, there is minimal overhead at runtime.
Even better, the generated code is perfectly readable, allowing you to verify the generated mapping code easily.

## Documentation

The documentation is available [here](https://mapperly.riok.app/docs/getting-started/installation).

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
public partial class CarMapper
{
    public partial CarDto CarToCarDto(Car car);
}

// Mapper usage
var mapper = new CarMapper();
var car = new Car { NumberOfSeats = 10, ... };
var dto = mapper.CarToCarDto(car);
dto.NumberOfSeats.ShouldBe(10);
```

[Read the docs](https://mapperly.riok.app/docs/getting-started/installation) for any further information.

## Support Policy

Only the latest version released on the stable channel is fully supported.
We strive to support all .NET versions that are currently supported by Microsoft.

## Upgrading

Find a list of breaking changes for each major version and upgrade guides [here](https://mapperly.riok.app/docs/category/upgrading/).

## How To Contribute

We would love for you to contribute to Mapperly and help make it even better than it is today!
Find information on how to contribute [in the docs](https://mapperly.riok.app/docs/contributing/).

## Professional Support

Need assistance with Mapperly or looking for consulting services? The riok team is available to help with:

- Architecture consultation
- Custom feature development and integration support
- Enterprise support and training
- Performance optimization and code reviews

Reach out through [GitHub Discussions](https://github.com/riok/mapperly/discussions) for questions, or contact the riok team directly for professional consulting services at [hello@riok.ch](mailto:hello@riok.ch). You can also support the project and get priority support through [GitHub Sponsors](https://github.com/sponsors/riok).

## License

Mapperly is [Apache 2.0](https://github.com/riok/mapperly/blob/main/LICENSE) licensed.
