# Static mappers and extension methods

Mapperly supports static mappers and extension methods:
```csharp
[Mapper]
public static partial class CarMapper
{
    public static partial CarDto CarToCarDto(this Car car);

    private static DateOnly DateTimeToDateOnly(DateTime dt) => DateOnly.FromDateTime(dt);
}
```

:::info
Mapperly does not support static partial mapping methods in non-static mapper classes.
:::
