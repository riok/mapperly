---
sidebar_position: 1
description: Create your first mapper with Mapperly.
---

# Create your first mapper

Create a mapper declaration as a partial class
and apply the `Riok.Mapperly.Abstractions.MapperAttribute` attribute.
Mapperly generates mapping method implementations for the defined mapping methods in the mapper.

```csharp title="Mapper declaration"
[Mapper]
public partial class CarMapper
{
    public partial CarDto CarToCarDto(Car car);
}
```

```csharp title="Mapper usage"
var mapper = new CarMapper();
var car = new Car { NumberOfSeats = 10, ... };
var dto = mapper.CarToCarDto(car);
dto.NumberOfSeats.Should().Be(10);
```
