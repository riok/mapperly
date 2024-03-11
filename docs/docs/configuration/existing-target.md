---
sidebar_position: 8
description: Map to an existing target object
---

# Existing target object

import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';

If an existing object instance should be used as target, you can define the mapping method as void with the target as second parameter:

```csharp title="Mapper declaration"
[Mapper]
public partial class CarMapper
{
    // highlight-start
    public partial void CarToCarDto(Car car, CarDto dto);
    // highlight-end
}
```

```csharp title="Mapper usage"
var mapper = new CarMapper();
var car = new Car { NumberOfSeats = 10, ... };
var dto = new CarDto();

mapper.CarToCarDto(car, dto);
dto.NumberOfSeats.Should().Be(10);
```

## Merge objects

To merge two objects together, `AllowNullPropertyAssignment` can be set to `false`.
This ignores all properties on the source with a `null` value.

<Tabs>
  <TabItem value="declaration" label="Declaration" default>

```csharp
// highlight-start
[Mapper(AllowNullPropertyAssignment = false)]
// highlight-end
static partial class FruitMapper
{
    public static partial void ApplyUpdate(this Fruit fruit, FruitUpdate update);
}

class Fruit { public required string Name { get; set; } public required string Color { get; set; } }
record FruitUpdate(string? Name, string? Color);
```

  </TabItem>
  <TabItem value="generated" label="Generated code" default>

```csharp
static partial class FruitMapper
{
    public static partial void Update(global::FruitUpdate update, global::Fruit fruit)
    {
        if (update.Name != null)
        {
            fruit.Name = update.Name;
        }
        if (update.Color != null)
        {
            fruit.Color = update.Color;
        }
    }
}
```

  </TabItem>
</Tabs>

See also [null value handling](./mapper.mdx#null-values).
