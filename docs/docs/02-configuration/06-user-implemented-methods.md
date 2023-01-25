# User implemented mapping methods

If Mapperly cannot generate a mapping, one can implement it manually simply by providing a method body in the mapper declaration:

```csharp
[Mapper]
public partial class CarMapper
{
    public partial CarDto CarToCarDto(Car car);

    private int TimeSpanToHours(TimeSpan t) => t.Hours;
}
```

Whenever Mapperly needs a mapping from `TimeSpan` to `int` inside the `CarMapper` implementation, it will use the provided implementation.
