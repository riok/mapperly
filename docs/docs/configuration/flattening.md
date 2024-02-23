---
sidebar_position: 2
description: Flatten properties and fields
---

# Flattening and unflattening

It is pretty common to flatten objects during mapping, eg. `Car.Make.Id => CarDto.MakeId`.
Mapperly tries to figure out flattenings automatically by making use of the pascal case C# notation.
If Mapperly can't resolve the target or source property correctly, it is possible to manually configure it by applying the `MapPropertyAttribute`
by either using the source and target property path names as arrays or using a dot separated property access path string

```csharp
[MapProperty([nameof(Car.Make), nameof(Car.Make.Id)], [nameof(CarDto.MakeId)])]
// Or alternatively
[MapProperty("Make.Id", "MakeId")]
// Or
[MapProperty($"{nameof(Make)}.{nameof(Make.Id)}", "MakeId")]
partial CarDto Map(Car car);
```

:::info
Unflattening is not automatically configured by Mapperly and needs to be configured manually via `MapPropertyAttribute`.
:::

## Experimental full `nameof`

Mapperly supports an experimental "fullnameof".
It can be used to configure property paths using `nameof`.
Opt-in is done by prefixing the path with `@`.

```csharp
[MapProperty(nameof(@Car.Make.Id), nameof(CarDto.MakeId))]
partial CarDto Map(Car car);
```

`@nameof(Car.Make.Id)` will result in the property path `Make.Id`.
The first part of the property path is stripped.
Make sure these property paths start with the type of the property and not with a namespace or a property.

:::warning
This is an experimental API.
Its API surface is not subject to semantic releases and may break in any release.
:::
