# Conversions

Mapperly implements several types of automatic conversions.
A list of conversions supported by Mapperly is available [here](../api/riok.mapperly.abstractions.mappingconversiontype#fields).

## Disable all automatic conversions

To disable all conversions supported by Mapperly set `EnabledConversions` to `None`:
```csharp
// highlight-start
[Mapper(EnabledConversions = MappingConversionType.None)]
// highlight-end
public partial class CarMapper
{
  ...
}
```

## Disable specific automatic conversions

To disable a specific conversion type, set `EnabledConversions` to `All` excluding the conversion type to disable:
```csharp
// this disables conversions using the ToString() method:
// highlight-start
[Mapper(EnabledConversions = MappingConversionType.All & ~MappingConversionType.ToStringMethod)]
// highlight-end
public partial class CarMapper
{
  ...
}
```
