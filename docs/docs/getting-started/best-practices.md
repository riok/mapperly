---
sidebar_position: 3
description: Best practices for using Mapperly effectively.
---

# Best practices

A collection of recommendations for getting the most out of Mapperly.

## Keep strict mappings enabled

Mapperly enables strict mappings by default.
This means all source and target members need to be mapped, otherwise a warning is emitted.
Keep this enabled to catch mapping issues at build time rather than at runtime.

## Treat Mapperly warnings as errors

Elevate warnings to errors for Release builds so that unmapped or misconfigured members break the build.
This can be done using [C# compiler options](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings)
or by configuring individual [Mapperly analyzer diagnostics](../configuration/analyzer-diagnostics/index.mdx).

## Write complex mappings by hand

When a mapping requires lots of configuration or complex logic,
implement it manually instead of trying to express everything through Mapperly attributes.
You can mix generated and [user-implemented mapping methods](../configuration/user-implemented-methods.mdx) in the same mapper without any issues.

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapOrder(Order order);

    // Complex mapping logic is easier to read and maintain when written by hand
    private MoneyDto MapMoney(Money money)
        => new MoneyDto { Amount = money.Units + money.Nanos / 1_000_000_000m, Currency = money.CurrencyCode };
}
```

## Do not put business logic in mappers

Mappers should only handle structural data transformation.
Business rules, validation, and computed values belong in dedicated services or domain logic.
Mixing business logic into mapping code makes it harder to test and reason about.

## Use separate mappers for requests and responses

When mapping between API models and domain objects,
maintain separate mappers: one for **domain objects → API responses** and another for **API requests → domain objects**.
This makes it straightforward to configure `RequiredMappingStrategy`:
use `Source` for the request mapper (all request fields must be consumed)
and `Target` for the response mapper (all response fields must be populated).

```csharp
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public partial class RequestMapper
{
    public partial CreateOrderCommand MapRequest(CreateOrderRequest request);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ResponseMapper
{
    public partial GetOrderResponse MapResponse(Order order);
}
```

## Use a consistent parameter name

Use a generic parameter name like `source` instead of a domain-specific name.
This makes the mapping direction clear at a glance,
especially when the source and target types have similar names (e.g. `Car` and `CarDto`).

```csharp
[Mapper]
public partial class CarMapper
{
    // highlight-start
    // Good: clear which parameter is the source
    public partial CarDto Map(Car source);
    // highlight-end

    // Avoid: unclear when reading the mapping configuration and/or generated implementation
    public partial CarDto Map(Car car);
}
```
