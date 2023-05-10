# Mapper configuration

The `MapperAttribute` provides options to customize the generated mapper class.

## Copy behaviour

By default, Mapperly does not create deep copies of objects to improve performance.
If an object can be directly assigned to the target, it will do so
(eg. if the source and target type are both `Car[]`, the array and its entries will not be cloned).
To create deep copies, set the `UseDeepCloning` property on the `MapperAttribute` to `true`.

```csharp
// highlight-start
[Mapper(UseDeepCloning = true)]
// highlight-end
public partial class CarMapper
{
  ...
}
```

## Properties / fields

On each mapping method declaration, property and field mappings can be customized.
If a property or field on the target has a different name than on the source, the `MapPropertyAttribute` can be applied.

```csharp
[Mapper]
public partial class CarMapper
{
    // highlight-start
    [MapProperty(nameof(Car.Model), nameof(CarDto.ModelName))]
    // highlight-end
    public partial CarDto ToDto(Car car);
}
```

### Ignore properties / fields

To ignore a property or field, the `MapperIgnoreTargetAttribute` or `MapperIgnoreSourceAttribute` can be used.

```csharp
[Mapper]
public partial class CarMapper
{
    // highlight-start
    [MapperIgnoreTarget(nameof(CarDto.MakeId))]
    [MapperIgnoreSource(nameof(Car.Id))]
    // highlight-end
    public partial CarDto ToDto(Car car);
}
```

### Property name mapping strategy

By default, property and field names are matched using a case sensitive strategy.
If all properties/fields differ only in casing, for example `ModelName` on the source
and `modelName` on the target,
the `MapperAttribute` can be used with the `PropertyNameMappingStrategy` option.

```csharp
// highlight-start
[Mapper(PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive)]
// highlight-end
public partial class CarMapper
{
    public partial CarDto ToDto(Car car);
}

public class Car
{
    public string ModelName { get; set; }
}

public class CarDto
{
    public string modelName { get; set; }
}
```

### Strict property mappings

To enforce strict mappings
(all source members have to be mapped to a target member
and all target members have to be mapped from a source member,
except for ignored members)
set the following two EditorConfig settings (see also [analyzer diagnostics](./15-analyzer-diagnostics.mdx)):

```editorconfig title=".editorconfig"
[*.cs]
dotnet_diagnostic.RMG012.severity = error # Unmapped target member
dotnet_diagnostic.RMG020.severity = error # Unmapped source member
```

### Strict enum mappings

To enforce strict enum mappings set `RMG037` and `RMG038` to error, see [strict enum mappings](./04-enum.mdx).
