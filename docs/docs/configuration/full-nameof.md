---
sidebar_position: 19
description: Nameof references
---

# Full `nameof`

Mapperly supports a "fullnameof".
It can be used to configure nested member paths or fully qualified names using `nameof`.
To determine the correct reference,
Mapperly tries to inteligently resolve the referenced target.
To use "fullnameof" instead of the regular C# `nameof`, one needs to opt-in by prefixing the member path with `@`.

```csharp
[MapProperty(nameof(@MyNamespace.Car.Make.Id), nameof(CarDto.MakeId))]
partial CarDto Map(Car car);
```

`nameof(@MyNamespace.Car.Make.Id)` will result in the property path `Make.Id`:

```csharp
[MapProperty("Make.Id", "MakeId")]
```

If the `@` is not used, the default C# behavior of `nameof` is used,
which would result in the property name `Id`, which is not what we want in this case:

```csharp
[MapProperty("Id", "MakeId")]
```
