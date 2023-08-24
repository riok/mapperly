---
sidebar_position: 1
description: Use static mappers and extension methods.
---

# Static mappers and extension methods

Mapperly supports static mappers and extension methods:

```csharp
[Mapper]
public static partial class CarMapper
{
    public static partial CarDto CarToCarDto(this Car car);

    private static int TimeSpanToHours(TimeSpan t) => t.Hours;
}
```

## Static methods in instantiable class

Static methods are supported in non-static mapper classes. This supports the static interface use case. When a static mapping method is present, to simplify mapping method resolution and reduce confusion about which mapping method Mapperly uses, all methods must be static.

```csharp
public interface ICarMapper
{
    static abstract CarDto ToDto(Car car);
}

[Mapper]
// highlight-start
public partial class CarMapper : ICarMapper
// highlight-end
{
// highlight-start
    public static partial CarDto ToDto(Car car);
// highlight-end
}
```
