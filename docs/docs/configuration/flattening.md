---
sidebar_position: 2
description: Flatten properties and fields
---

# Flattening and unflattening

It is pretty common to flatten objects during mapping, eg. `Car.Make.Id => CarDto.MakeId`, or unflatten, eg. `CarDto.MakeId => Car.Make.Id`.
Mapperly tries to figure out flattenings and unflattenings automatically by making use of the pascal case C# notation.
If Mapperly can't resolve the target or source property correctly, it is possible to manually configure it by applying the `MapPropertyAttribute`
by either using the source and target property path names as arrays or using a dot separated property access path string

```csharp
[MapProperty(new[] { nameof(Car.Make), nameof(Car.Make.Id) }, new[] { nameof(CarDto.MakeId) })]
// Or alternatively
[MapProperty("Make.Id", "MakeId")]
partial CarDto Map(Car car);
```
