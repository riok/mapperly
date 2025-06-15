---
sidebar_position: 17
description: Including Mapping Configurations
---

# Including Mapping Configurations

Mapperly supports reusing mapping configurations across different mapping methods using
`IncludeMappingConfigurationAttribute` attribute. This enables sharing and modularizing mapping logic for consistent
mapping behavior between multiple methods.

## Defining and Reusing Mapping Configurations

### Including an Existing Mapping Configuration

To include an existing mapping configuration in another mapping method, use the `IncludeMappingConfigurationAttribute`,
providing the method name:

```csharp
// Property mapping configurations
public partial static void CopyApple(AppleDto dto, Apple apple);

[IncludeMappingConfiguration(nameof(CopyApple))] 
public partial static AppleDto ToApple(Apple apple);
```

This includes the following configurations:
- `MapProperty`
- `MapPropertyFromSource`
- `MapperIgnoreTarget`
- `MapperIgnoreSource`
- `MapperIgnoreObsoleteMembers`
- `MapperRequiredMapping`
- `MapValue`
- `MapDerivedType`

### Including the Mapping configuration from base class

This attribute also supports including
the configurations of the base class. Suppose you want to map
`Apple` to `AppleDto` and the mapping logic is defined for its base classes `Fruit` to `FruitDto`.
Here's how you can do it:

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

This attribute can only include such mapping configurations that are in the same class.
Currently, configurations from other classes cannot be included.

This attribute only uses a mapping if the mapped types are the same or base types of
the mapped type.

If the attribute includes such configuration that causes collision, then it is reported as an error.

## Diagnostics

If an `IncludeMappingConfigurationAttribute` refers to an ambiguous mapping configuration (e.g., multiple 
configurations exist with the same name), the mapper will emit RMG062 to help you resolve the ambiguity. 
This configuration only considers the compatible mappings.

