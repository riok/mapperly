# Mapperly

![Nuget](https://img.shields.io/nuget/v/Riok.Mapperly?style=flat-square)
![GitHub](https://img.shields.io/github/license/riok/mapperly?style=flat-square)

Mapperly is a .NET source generator for generating object mappings. Inspired by MapStruct.

Because Mapperly creates the mapping code at build time, there is minimal overhead at runtime.
Even better, the generated code is perfectly readable, allowing you to verify the generated mapping code easily.

Mapperly is the fastet .NET object mapper out there, surpassing even the naive manual mapping approach! The benchmark was generated with https://github.com/mjebrahimi/Benchmark.netCoreMappers.

|        Method |       Mean |   Error |  StdDev |  Gen 0 | Allocated |
|-------------- |-----------:|--------:|--------:|-------:|----------:|
|   AgileMapper | 1,523.8 ns | 3.90 ns | 3.25 ns | 1.5106 |   3,160 B |
|    TinyMapper | 4,094.3 ns | 3.90 ns | 3.05 ns | 1.0300 |   2,160 B |
| ExpressMapper | 2,595.8 ns | 5.49 ns | 5.14 ns | 2.3422 |   4,904 B |
|    AutoMapper | 1,203.9 ns | 2.30 ns | 2.15 ns | 0.9098 |   1,904 B |
| ManualMapping |   529.6 ns | 0.52 ns | 0.44 ns | 0.5541 |   1,160 B |
|       Mapster |   562.1 ns | 1.14 ns | 0.89 ns | 0.9098 |   1,904 B |
|      Mapperly |   338.5 ns | 0.95 ns | 0.84 ns | 0.4396 |     920 B |

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

## Configuration

The attributes defined in `Riok.Mapperly.Abstractions` can be used to customize different aspects of the generated mapper.

### Mapper

The `MapperAttribute` provides options to customize the generated mapper class.
The default enum mapping and null handling strategy are adjustable.

####  Copy behaviour

By default, Mapperly does not create deep copies of objects to improve performance.
If an object can be directly assigned to the target, it will do so
(eg. if the source and target type are both `Car[]`, the array and its entries will not be cloned).
To create deep copies, set the `UseDeepCloning` property on the `MapperAttribute` to `true`.

### Properties

On each mapping method declaration, property mappings can be customized.
If a property on the target has a different name than on the source, the `MapPropertyAttribute` can be applied.
If a property should be ignored, the `MapperIgnoreAttribute` can be used.

#### Flattening and unflattening

It is pretty common to flatten objects during mapping, eg. `Car.Make.Id => CarDto.MakeId`.
Mapperly tries to figure out flattenings automatically by making use of the pascal case C# notation.
If Mapperly can't resolve the target or source property correctly, it is possible to manually configure it by applying the `MapPropertyAttribute`
by either using the source and target property path names as arrays or using a dot separated property access path string
```c#
[MapProperty(Source = new[] { nameof(Car), nameof(Car.Make), nameof(Car.Make.Id) }, Target = new[] { nameof(CarDto), nameof(CarDto.MakeId) })]
// Or alternatively
[MapProperty(Source = "Car.Make.Id", Target = "CarDto.MakeId")]
```
Note: Unflattening is not yet automatically configured by Mapperly and needs to be configured manually via `MapPropertyAttribute`.

### Enum

The enum mapping can be customized by setting the strategy to use.
Apply the `MapEnumAttribute` and pass the strategy to be used for this enum.
It is also possible to set the strategy for the entire mapper via the `MapperAttribute`.
Available strategies:

| Name    | Description                               |
|---------|-------------------------------------------|
| ByValue | Matches enum entries by their values      |
| ByName  | Matches enum entries by their exact names |

The `IgnoreCase` property allows to opt in for case insensitive mappings.

## Static mappers and extension methods

Mapperly supports static mappers and extension methods:
```c#
[Mapper]
public static partial class DtoMapper
{
    public static partial CarDto CarToCarDto(this Car car);

    private static DateOnly DateTimeToDateOnly(DateTime dt) => DateOnly.FromDateTime(dt);
}
```

> Mapperly does not support static partial mapping methods in non-static mapper classes (yet).

## User implemented mapping methods

If Mapperly cannot generate a mapping, one can implement it manually simply by providing a method body in the mapper declaration:

```c#
[Mapper]
public partial class DtoMapper
{
    public partial CarDto CarToCarDto(Car car);

    private DateOnly DateTimeToDateOnly(DateTime dt) => DateOnly.FromDateTime(dt);
}
```

Whenever Mapperly needs a mapping from `DateTime` to `DateOnly` inside the `DtoMapper` implementation, it will use the provided implementation.

## Constructor mappings

Mapperly supports using parameterized constructors of mapping target types.
Mapperly resolves the constructor to be used by the following priorities:
* accessible constructors annotated with `MapperConstructor`
* accessible parameterless constructors
* accessible constructors ordered in descending by their parameter count

The first constructor which allows the mapping of all parameters is used.
Constructor parameters are mapped in a case insensitive matter.

## Before / after map

To run custom code before or after a mapping, the generated map method can be wrapped in a custom method:

```c#
[Mapper]
public partial class DtoMapper
{
    public CarDto MapCarToCarDto(Car car)
    {
        // custom before map code...
        var dto = CarToCarDto(car);
        // custom after map code...
        return dto;
    }

    private partial CarDto CarToCarDto(Car car);
}
```

## Void mapping methods

If an existing object instance should be used as target, you can define the mapping method as void with the target as second parameter:

```c#
// Mapper declaration
[Mapper]
public partial class DtoMapper
{
    public partial void CarToCarDto(Car car, CarDto dto);
}

// Mapper usage
var mapper = new DtoMapper();
var car = new Car { NumberOfSeats = 10, ... };
var dto = new CarDto();
mapper.CarToCarDto(car, dto);
dto.NumberOfSeats.Should().Be(10);
```
