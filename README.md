# Mapperly

A .NET source generator for generating object mappings.
Inspired by MapStruct.

## Quickstart

### Installation

Add the NuGet Package to your project:
```bash
dotnet add package Riok.Mapperly
```

### Create your first mapper

Create a mapper declaration as an interface or abstract class
and apply the `Riok.Mapperly.Abstractions.MapperAttribute` attribute.
Mapperly generates mapping method implementations for the defined mapping methods in the mapper.
The default implementation name is built removing the leading `I` if it is an interface (`IDtoMapper` => `DtoMapper`) and appending `Impl` if the mapper definition is an abstract class or the interface name does not start with an `I`.
A mapper instance is available through the `Instance` field on the generated implementation.

```c#
// mapper declaration
[Mapper]
public interface IDtoMapper
{
    CarDto CarToCarDto(Car car);
}

// mapper usage
var car = new Car { NumberOfSeats = 10, ... };
var dto = DtoMapper.Instance.CarToCarDto();
dto.NumberOfSeats.Should().Be(10);
```

### Configuration

The attributes defined in `Riok.Mapperly.Abstractions` can be used to customize different aspects of the generated mapper.

#### Mapper

The `MapperAttribute` provides options to customize the generated mapper class.
The generated class name, the instance field name and the default enum mapping strategy is adjustable.

### Copy behaviour

By default, Mapperly does not create deep copies of objects to improve performance.
If an object can be directly assigned to the target, it will do so
(eg. if the source and target type are both `Car[]`, the array and its entries will not be cloned).
To create deep copies, set the `UseDeepCloning` property on the `MapperAttribute` to `true`.

#### Properties

On each mapping method declaration property mappings can be customized.
If a property on the target has a different name than on the source, the `MapPropertyAttribute` can be applied.
Flattening is not yet supported.
If a property should be ignored, the `MapperIgnoreAttribute` can be used.

#### Enum

The enum mapping can be customized by setting the strategy to use.
Apply the `MapEnumAttribute` and pass the strategy to be used for this enum.
It is also possible to set the strategy for the entire mapper via the `MapperAttribute`.
Available strategies:

| Name    | Description                               |
|---------|-------------------------------------------|
| ByValue | Matches enum entries by their values      |
| ByName  | Matches enum entries by their exact names |

The `IgnoreCase` property allows to opt in for case ignored mappings.

## Void mapping methods

If an existing object instance should be used as target you can define the mapping method as void with the target as second parameter:

```c#
// mapper declaration
[Mapper]
public interface IDtoMapper
{
    void CarToCarDto(Car car, CarDto dto);
}

// mapper usage
var car = new Car { NumberOfSeats = 10, ... };
var dto = new CarDto();
DtoMapperImpl.Instance.CarToCarDto(car, dto);
dto.NumberOfSeats.Should().Be(10);
```

## User implemented mapping methods

If Mapperly cannot generate a mapping, one can implement it manually simply by providing a method body in the mapper declaration:

```c#
[Mapper]
public interface IDtoMapper
{
    CarDto CarToCarDto(Car car);

    DateOnly DateTimeToDateOnly(DateTime dt) => DateOnly.FromDateTime(dt);
}
```

Whenever Mapperly needs a mapping from `DateTime` to `DateOnly` inside the `IDtoMapper` implementation it will use the provided implementation.
