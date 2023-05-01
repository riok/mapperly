# Runtime target type mapping

If the target type of a mapping is not known at compile time,
a mapping method with a `Type` parameter can be used.
Mapperly implements this mapping method
using all mappings the user defined in the mapper.

```csharp
[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    public static partial object Map(object source, Type targetType);
    // highlight-end

    private static partial BananaDto MapBanana(Banana source);
    private static partial AppleDto MapApple(Apple source);
}

class Banana {}
class Apple {}

class BananaDto {}
class AppleDto {}
```

If the source or target type of a runtime target type mapping is not `object`,
only user mappings of which the source/target type is assignable to the source/target type of the mapping method are considered.

Runtime target type mappings support [derived type mappings](./10-derived-type-mapping.md).
The `MapDerivedTypeAttribute` can be directly applied to a runtime target type mapping method.

:::info
Mapperly runtime target type mappings
only support source/target type combinations which are defined
as mappings in the same mapper.
If an unknown source/target type combination is provided at runtime,
an `ArgumentException` is thrown.
:::
