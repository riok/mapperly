# Object Factories

By default the generated code will instantiate objects using the default parameterless constructor if available.
If no parameterless constructor is available, Mapperly will try to map to the constructor arguments (see [constructor mappings](./07-ctor-mappings.md)).

Alternatively custom object factories can be used to construct or resolve target objects.
To make use of object factories create an object factory method inside the mapper class
and set the `Riok.Mapperly.Abstractions.ObjectFactoryAttribute` attribute.
An object factory method needs to be parameterless and needs to return a non-void type.
The first object factory with a matching return type is used to construct the desired type by Mapperly.

:::info
If an object factory is used for a certain type,
Mapperly cannot map to init only properties or constructor parameters.
:::

```csharp
[Mapper]
public partial class CarMapper
{
    // highlight-start
    [ObjectFactory]
    private CarDto CreateCarDto()
        => CarDto.CreateFromCustomMethod();
    // highlight-end

    public partial CarDto CarToCarDto(Car car);
}
```

The generated code will look like this:
```csharp
public partial class CarMapper
{
    public partial CarDto CarToCarDto(Car car)
    {
        // highlight-start
        var target = CreateCarDto();
        // highlight-end
        // map all properties...
        return target;
    }
}
```

## Generic object factory

Mapperly also supports generic object factories with or without type constraints.
A generic object factory needs to be parameterless
and is required to have exactly one type parameter which is also the return type.

```csharp
[Mapper]
public partial class CarMapper
{
    // highlight-start
    // an object factory which will create instances of CarDto and all it's subclasses
    [ObjectFactory]
    private T CreateCar<T>()
        where T : CarDto
        => CarDto.CreateFromCustomMethod();

    // or an object factory which will create objects for all types
    [ObjectFactory]
    private T Create<T>()
        => _diContainer.Resolve<T>();
    // highlight-end

    public partial CarDto CarToCarDto(Car car);
}
```
