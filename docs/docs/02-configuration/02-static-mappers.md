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
