---
sidebar_position: 4
description: Run custom logic before or after the generated mapping.
---

# Before / after map

To run custom code before or after a mapping, the generated map method can be wrapped in a custom method:

```csharp
[Mapper]
public partial class CarMapper
{
    private partial CarDto CarToCarDto(Car car);

    // highlight-start
    // Default ensures Mapperly uses this mapping whenever a conversion
    // from Car to CarDto is needed instead of the `CarToCarDto` method.
    [UserMapping(Default = true)]
    public CarDto MapCarToCarDto(Car car)
    {
        // custom before map code...
        var dto = CarToCarDto(car);
        // custom after map code...
        return dto;
    }
    // highlight-end
}
```
