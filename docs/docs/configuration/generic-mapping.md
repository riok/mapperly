---
sidebar_position: 13
description: Create a generic mapping method
---

# Generic mappings

Mapperly supports generic user defined mapping methods.
Mapperly implements this mapping method
using all mappings the user defined in the mapper that satisfy the source and target type constraints.

```csharp
[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    public static partial TTarget MapFruit<TTarget>(Fruit source);
    // highlight-end

    private static partial BananaDto MapBanana(Banana source);
    private static partial AppleDto MapApple(Apple source);
}

class Fruit {}
class Banana : Fruit {}
class Apple : Fruit {}

class BananaDto {}
class AppleDto {}
```

## Runtime target type parameter

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

## Type constraints

If the source or target type of a runtime target type mapping is not `object` or the generic type has constraints,
only user mappings of which the source/target type is assignable to the source/target type of the mapping method are considered.

Generic mappings and runtime target type parameter mappings support [derived type mappings](./derived-type-mapping.md).
The `MapDerivedTypeAttribute` can be directly applied to a mapping method.

:::info
Mapperly generic mappings and runtime target type parameter mappings
only support source/target type combinations which are defined
as mappings in the same mapper.
If an unknown source/target type combination is provided at runtime,
an `ArgumentException` is thrown.
:::

## Existing target

Generic mappings can also be used with [existing target objects](./existing-target.mdx).
The mapping method should return `void` and accept the target as a second parameter.

```csharp
[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    public static partial void Map<TSource, TTarget>(TSource source, TTarget target);
    // highlight-end

    private static partial void MapBanana(Banana source, BananaDto target);
    private static partial void MapApple(Apple source, AppleDto target);
}

class Banana {}
class Apple {}

class BananaDto {}
class AppleDto {}
```

The generic source and/or target type parameters can also have type constraints:

```csharp
[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    public static partial void Map<TSource, TTarget>(TSource source, TTarget target)
        where TSource : Fruit
        where TTarget : FruitDto;
    // highlight-end

    private static partial void MapBanana(Banana source, BananaDto target);
    private static partial void MapApple(Apple source, AppleDto target);
}

class Fruit {}
class Banana : Fruit {}
class Apple : Fruit {}

class FruitDto {}
class BananaDto : FruitDto {}
class AppleDto : FruitDto {}
```
