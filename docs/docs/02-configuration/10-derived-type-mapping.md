# Derived types and interfaces

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

Mapperly supports interfaces and base types as mapping sources and targets,
but Mapperly needs to know which derived types exist.
This can be configured with the `MapDerivedTypeAttribute`:

<!-- do not indent this, it won't work, https://stackoverflow.com/a/67579641/3302887 -->

<Tabs>
<TabItem value="declaration" label="Declaration" default>

```csharp
[Mapper]
public static partial class ModelMapper
{
    // highlight-start
    [MapDerivedType<Audi, AudiDto>] // for c# language level ≥ 11
    [MapDerivedType(typeof(Porsche), typeof(PorscheDto))] // for c# language level < 11
    // highlight-end
    public static partial CarDto MapCar(Car source);
}

abstract class Car {}
class Audi : Car {}
class Porsche : Car {}

abstract class CarDto {}
class AudiDto : CarDto {}
class PorscheDto : CarDto {}
```

</TabItem>
<TabItem value="generated" label="Generated code" default>

```csharp
[Mapper]
public static partial class ModelMapper
{
    public static partial CarDto MapCar(Car source)
    {
        return source switch
        {
            Audi x => MapToAudiDto(x),
            Porsche x => MapToPorscheDto(x),
            _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to CarDto as there is no known derived type mapping", nameof(source)),
        };
    }

    // ... implementations of MapToAudiDto and MapToPorscheDto
}
```

</TabItem>
</Tabs>

All source types provided to the `MapDerivedTypeAttribute`
need to implement or extend the type of the mapping method parameter.
All target types provided to the `MapDerivedTypeAttribute`
need to implement or extend the mapping method return type.
Each source type has to be unique but multiple source types can be mapped to the same target type.
