# Before / after map

To run custom code before or after a mapping, the generated map method can be wrapped in a custom method:

```csharp
[Mapper]
public partial class DtoMapper
{
    public CarDto MapCarToCarDto(Car car)
    {
        // custom before map code...
        var dto = CarToCarDto(car);
        // custom after map code...
        return dto;
    }

    private partial CarDto CarToCarDto(Car car);
}
```
