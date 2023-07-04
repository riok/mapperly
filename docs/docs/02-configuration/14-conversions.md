# Conversions

Mapperly implements several types of automatic conversions (in order of priority):

| Name                 | Description                                                                                                               | Conditions                                                                                                      |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Direct assignment    | Directly assigns the source object to the target                                                                          | Source type is assignable to the target type and `UseDeepCloning` is `false`                                    |
| Queryable            | Projects the source queryable to the target queryable                                                                     | Source and target types are `IQueryable<>`                                                                      |
| Dictionary           | Maps a source dictionary to an enumerable target                                                                          | Source type is an `IDictionary<,>` or an `IReadOnlyDictionary<,>`                                               |
| Enumerable           | Maps an enumerable source to an enumerable target                                                                         | Source type is an `IEnumerable<>`                                                                               |
| Span                 | Maps a `Span<>`, `ReadOnlySpan<>` to or from `Span<>`, `ReadOnlySpan<>` or enumerable                                     | Source or target type is a `Span<>`, `ReadOnlySpan<>`                                                           |
| Memory               | Maps a `Memory<>`, `ReadOnlyMemory<>` to or from `Memory<>`, `ReadOnlyMemory<>`, `Span<>`, `ReadOnlySpan<>` or enumerable | Source or target type is a `Memory<>` or `ReadOnlyMemory<>`                                                     |
| Implicit cast        | Implicit cast operator                                                                                                    | An implicit cast operator is defined to cast from the source type to the target type                            |
| Parse method         | Uses a static `Parse` method on the source type                                                                           | Source type is a `string` and target has a static method with the following signature: `TTarget Parse(string)`. |
| Constructor          | Uses a constructor on the target type with the source as single parameter                                                 | Target type has a visible constructor with a single parameter of the source type.                               |
| String to enum       | Maps a string to an enum member name                                                                                      | Source type is a `string` and the target type is an enum                                                        |
| Enum to string       | Maps an enum member name to a string                                                                                      | Source type is an enum and the target type is a `string`                                                        |
| Enum to enum         | Maps an enum to another enum either by value or by member name                                                            | Source and target types are enums                                                                               |
| DateTime to DateOnly | Maps a `DateTime` to a `DateOnly`                                                                                         | Source type is a `DateTime` and target type is a `DateOnly`                                                     |
| DateTime to TimeOnly | Maps a `DateTime` to a `TimeOnly`                                                                                         | Source type is a `DateTime` and target type is a `TimeOnly`                                                     |
| Explicit cast        | Explicit cast operator                                                                                                    | An explicit cast operator is defined to cast from the source type to the target type                            |
| ToString             | `ToString` method of an object                                                                                            | Target type is a `string`                                                                                       |
| New instance         | Create a new instance of the target type and map all properties                                                           | The target type has a visible constructor or an object factory exists for the target type                       |

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

## Enable only specific automatic conversions

To enable only specific conversion types, set `EnabledConversions` the conversion type to enable:

```csharp
// This disables conversions using the ToString() method, which is enabled by default:
// highlight-start
[Mapper(EnabledConversions = MappingConversionType.Constructor | MappingConversionType.ExplicitCast)]
// highlight-end
public partial class CarMapper
{
  ...
}
```
