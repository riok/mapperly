---
sidebar_position: 3
description: Flatten properties and fields
---

# Flattening and unflattening

It is pretty common to flatten objects during mapping, eg. `Car.Make.Id => CarDto.MakeId`.
Mapperly tries to figure out flattenings automatically by making use of the PascalCase C# notation.
If Mapperly can't resolve the target or source property correctly, it is possible to manually configure it by applying the `MapPropertyAttribute`
by either using the source and target property path names as arrays or using a dot separated property access path string

```csharp
[MapProperty([nameof(Car.Make), nameof(Car.Make.Id)], nameof(CarDto.MakeId))]
// Or alternatively
[MapProperty("Make.Id", "MakeId")]
// Or
[MapProperty($"{nameof(Make)}.{nameof(Make.Id)}", "MakeId")]
partial CarDto Map(Car car);
```

:::info
Unflattening is not automatically configured by Mapperly and needs to be configured manually via `MapPropertyAttribute`.
:::

## Flatten all members of property

If a property has many members that need to be flattened but that cannot be figured out automatically, this can be configured using the `MapNestedProperties` attribute.
This will bring all sub-members of a specified member into scope as if they were defined on the source object:

```csharp
[MapProperty([nameof(Car.Engine), nameof(Car.Engine.Horsepower)], nameof(CarDto.Horsepower))]
[MapProperty([nameof(Car.Engine), nameof(Car.Engine.FuelType)], nameof(CarDto.FuelType))]
[MapProperty([nameof(Car.Engine), nameof(Car.Engine.Cylinders)], nameof(CarDto.Cylinders))]
// Is equivalent to:
// highlight-start
[MapNestedProperties(nameof(Car.Engine))]
// highlight-end
partial CarDto Map(Car car);
```

:::info
The nested members have a lower priority than all immediate members of the source object.
E.g. if both the `Car` and the `Engine` classes in the preceding example contained an `Id` property, the `CarDto.Id` property would be mapped from `Car.Id`.

Similarly, the [automatic flattening](#flattening-and-unflattening) has precedence over the nested members:
`CarDto.EngineId` would be mapped from `Car.Engine.Id` rather than `Car.Engine.EngineId` if `Engine` contained both.
:::

:::warning
If multiple `MapNestedProperties` are defined that contain members that match to the same member on the target object, no guarantees are made as to which source member is chosen.
In such a case it is therefore recommended to define the expected property mapping explicitly using a `MapProperty` attribute.
:::

## Full `nameof`

Mapperly supports a "fullnameof" to simplify these configurations, see [full-nameof](./full-nameof.md).
