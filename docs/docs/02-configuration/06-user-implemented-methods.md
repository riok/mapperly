# User implemented mapping methods

If Mapperly cannot generate a mapping, one can implement it manually simply by providing a method body in the mapper declaration:

```csharp
[Mapper]
public partial class CarMapper
{
    public partial CarDto CarToCarDto(Car car);

    private DateOnly DateTimeToDateOnly(DateTime dt) => DateOnly.FromDateTime(dt);
}
```

Whenever Mapperly needs a mapping from `DateTime` to `DateOnly` inside the `CarMapper` implementation, it will use the provided implementation.
