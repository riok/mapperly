# Reference handling

Mapperly can support mapping object structures with circular references.
To opt in for reference handling set `UseReferenceHandling` to `true`:
```csharp
// highlight-start
[Mapper(UseReferenceHandling = true)]
// highlight-end
public partial class CarMapper
{
    public partial void CarToCarDto(Car car, CarDto dto);
}
```

This enables the usage of a default reference handler
which reuses the same target object instance if encountered the same source object instance.

## Custom reference handler

To use a custom `IReferenceHandler` implementation,
a parameter of the type `Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler`
annotated with the `Riok.Mapperly.Abstractions.ReferenceHandling.ReferenceHandlerAttribute`
can be added to the mapping method.

```csharp
// highlight-start
[Mapper(UseReferenceHandling = true)]
// highlight-end
public partial class CarMapper
{
    // highlight-start
    public partial void CarToCarDto(Car car, CarDto dto, [ReferenceHandler] IReferenceHandler myRefHandler);
    // highlight-end
}
```

## User implemented mappings

To make use of the `IReferenceHandler` in a user implemented mapping method,
add a parameter as described in the section "[Custom reference handler](#custom-reference-handler)".
