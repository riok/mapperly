sidebar_position: 17
description: Include settings from another mapper method
---

# Including Mapping Configurations

Mapperly supports reusing mapping configurations across different mapping methods using two new attributes:
`NamedMappingAttribute` and `IncludeMappingConfigurationAttribute`. This enables sharing and modularizing mapping logic for consistent mapping behavior between multiple methods.

## Defining and Reusing Mapping Configurations

### 1. Naming a Mapping Configuration

By default, every mapping method can be referenced by its method name as a mapping configuration. To explicitly assign a custom name, apply the `NamedMappingAttribute`

```csharp
[NamedMapping("CustomFruitMapping")] 
private partial FruitDto ToFruit(Fruit fruit);
```

### 2. Including an Existing Mapping Configuration

To include an existing mapping configuration in another mapping method, use the `IncludeMappingConfigurationAttribute`,
providing either the configuration name or the method name:

```csharp
[IncludeMappingConfiguration("CustomFruitMapping")] 
public partial static AppleDto Map(Apple apple);
``` 

Or refer to a method directly:

```csharp
[IncludeMappingConfiguration(nameof(ToFruit))] 
public partial static AppleDto Map(Apple apple);
```

> **Note:** The `IncludeMappingConfigurationAttribute` only uses a mapping if the mapped types are the same or base
> types of the mapped type. This means that the configuration will only be applied if the source and target types of
> the included mapping are compatible. Specifically, the types must either be the same, or the included mapping’s
> types must be base types of the method where the attribute is applied.


### Usage Example

Suppose you want to map `Apple` to `AppleDto` but reuse the mapping logic defined for `Fruit` to `FruitDto`. Here's how
you can do it:

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

In this example, the `Map` method for `Apple` reuses the configuration from `ToFruit`, ensuring consistent property mapping.

## Diagnostics

If an `IncludeMappingConfigurationAttribute` refers to an ambiguous mapping configuration (e.g., multiple configurations exist with the same name), the mapper will emit a diagnostic message to help you resolve the ambiguity. This configuration only considers the compatible mappings.

