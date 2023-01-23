# Constructor mappings

Mapperly supports using parameterized constructors of mapping target types.
Mapperly resolves the constructor to be used by the following priorities:

- accessible constructors annotated with `MapperConstructor`
- accessible parameterless constructors
- accessible constructors ordered in descending by their parameter count
- constructors with a `System.ObsoleteAttribute` attribute, unless they have a `MapperConstructor` attribute

The first constructor which allows the mapping of all parameters is used.
Constructor parameters are mapped in a case insensitive matter.

## Manual mapping configuration

Manual mapping of source properties to constructor parameters can be configured with the `MapProperty` attribute.
See example below

```csharp
public class Car
{
    // highlight-start
    public string ModelName { get; set; }
    // highlight-end
}

public record CarDto(string Model);

[Mapper]
public partial class CarMapper
{
    // highlight-start
    [MapProperty(nameof(Car.ModelName), nameof(CarDto.Model))]
    // highlight-end
    public partial CarDto ToDto(Car car);
}
```

If the target type is a normal class, where the constructor parameter name not necessarily matches the property name, you can identify the correct parameter name using a string literal.

```csharp
public class Car
{
    public string ModelName { get; set; }
}

public class CarDto
{
    // highlight-start
    public CarDto(string model)
    // highlight-end
    {
        ModelName = model;
    }

    public string ModelName { get; }
}

[Mapper]
public partial class CarMapper
{
    // highlight-start
    [MapProperty(nameof(Car.ModelName), "model")]
    // highlight-end
    public partial CarDto ToDto(Car car);
}
```
