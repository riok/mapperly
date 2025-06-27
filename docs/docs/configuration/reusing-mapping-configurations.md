---
sidebar_position: 17
description: Reusing Mapping Configurations
---

# Reusing Mapping Configurations

Mapperly supports reusing mapping configurations across different mapping methods using the
`IncludeMappingConfigurationAttribute`. This enables you to share and modularize mapping logic for consistent
behavior across multiple methods.

## Defining and Reusing Mapping Configurations

### Including an Existing Mapping Configuration

To include an existing mapping configuration in another mapping method, use the `IncludeMappingConfigurationAttribute`
and provide the method name:

```csharp
// Property mapping configurations
public partial static void CopyApple(AppleDto dto, Apple apple);

// highlight-start
[IncludeMappingConfiguration(nameof(CopyApple))]
// highlight-end
public partial static AppleDto ToApple(Apple apple);
```

Or use the provided mapper name:

```csharp
// Property mapping configurations
// highlight-start
[NamedMapping("CustomToApple")]
// highlight-end
public partial static void CopyApple(AppleDto dto, Apple apple);

// highlight-start
[IncludeMappingConfiguration("CustomToApple")]
// highlight-end
public partial static AppleDto ToApple(Apple apple);
```

The following configurations will be included:

- `MapProperty`
- `MapPropertyFromSource`
- `MapperIgnoreTarget`
- `MapperIgnoreSource`
- `MapperIgnoreObsoleteMembers`
- `MapperRequiredMapping`
- `MapValue`
- `MapDerivedType`

### Including a Mapping Configuration from a Base Class

This attribute also supports including configurations from a base class. For example, suppose you want to map
`Apple` to `AppleDto` and the mapping logic is defined for the base classes `Fruit` to `FruitDto`.
Here's how to set it up:

```csharp
class Fruit
{
    public string Name { get; set; }
    public decimal PricePerUnit { get; set; }
}

class FruitDto
{
    public string Title { get; set; }
}

class Apple : Fruit
{
    public int Weight { get; set; }
}

class AppleDto : FruitDto
{
    public int Weight { get; set; }
}

[Mapper]
public partial class MyMapper {
    [MapProperty(nameof(Fruit.Name), nameof(FruitDto.Title))]
    [MapperIgnoreSource(nameof(Fruit.PricePerUnit))]
    private partial FruitDto ToFruit(Fruit fruit);

    [IncludeMappingConfiguration(nameof(ToFruit))]
    public partial static AppleDto Map(Apple apple);
}
```

## Restrictions

- This attribute can only include mapping configurations defined in the same class. Configurations from other classes currently cannot be included.
- The attribute only reuses a mapping if the mapped types are the same or a base type of the mapped type.
- If the attribute includes configurations that cause a collision, it is reported as an error.

## Diagnostics

If an `IncludeMappingConfigurationAttribute` refers to an ambiguous mapping configuration (e.g., multiple
configurations exist with the same name), the mapper emits RMG062 to help you resolve the ambiguity.
This can be easily resolved by providing a custom name for the target mapping with `NamedMapping` attribute.
