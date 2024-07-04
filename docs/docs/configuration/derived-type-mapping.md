---
sidebar_position: 10
description: Map derived types and interfaces
---

# Derived types and interfaces

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

Mapperly supports interfaces and base types as mapping sources and targets, for both new instance and [existing target](./existing-target.mdx) mappings.
To do this, Mapperly needs to know which derived types exist.
This can be configured with the `MapDerivedTypeAttribute`:

<Tabs>
  <TabItem value="declaration" label="Declaration" default>
  
  ```csharp
  [Mapper]
  public static partial class ModelMapper
  {
      // highlight-start
      [MapDerivedType<Banana, BananaDto>] // for c# language level ≥ 11
      [MapDerivedType(typeof(Apple), typeof(AppleDto))] // for c# language level < 11
      // highlight-end
      public static partial FruitDto MapFruit(Fruit source);
  }
  
  abstract class Fruit {}
  class Banana : Fruit {}
  class Apple : Fruit {}
  
  abstract class FruitDto {}
  class BananaDto : FruitDto {}
  class AppleDto : FruitDto {}
  ```
  
  </TabItem>
  <TabItem value="generated" label="Generated code" default>
  
  ```csharp
  [Mapper]
  public static partial class ModelMapper
  {
      public static partial FruitDto MapFruit(Fruit source)
      {
          return source switch
          {
              Banana x => MapToBananaDto(x),
              Apple x => MapToAppleDto(x),
              _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to FruitDto as there is no known derived type mapping", nameof(source)),
          };
      }
  
      // ... implementations of MapToBananaDto and MapToAppleDto
  }
  ```
  
  </TabItem>
</Tabs>

All source types provided to the `MapDerivedTypeAttribute`
need to implement or extend the type of the mapping method parameter.
All target types provided to the `MapDerivedTypeAttribute`
need to implement or extend the mapping method return type.
Each source type has to be unique but multiple source types can be mapped to the same target type.

Configuration attributes on methods with `MapDerivedTypeAttribute`s are used to build
the mapping of each derived types combination unless there is a user defined mapping method for exactly
this source/target type combination.
