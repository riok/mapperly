---
sidebar_position: 12
description: Private member mapping
---

# Private member mapping

As of .NET 8.0, Mapperly supports mapping members that are normally inaccessible like `private` or `protected` properties. This is made possible by using the [UnsafeAccessorAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.unsafeaccessorattribute) which lets Mapperly access normally inaccessible members with zero overhead while being completely AOT safe.

By default `IncludedMembers` is set to `MemberVisibility.AllAccessible` which will configure Mapperly to map members of all accessibility levels as long as they are ordinarily accessible. To enable unsafe accessor usage, set `IncludedMembers` to `MemberVisibility.All`. Mapperly will then try to map members of all accessibilities, including ones that are not usually visible to external types.

```csharp
public class Fruit
{
    private bool _isSeeded;

    public string Name { get; set; }

    private int Sweetness { get; set; }
}

// highlight-start
[Mapper(IncludedMembers = MemberVisibility.All)]
// highlight-end
public partial class FruitMapper
{
    public partial FruitDto ToDto(Fruit source);
}
```

## Generated unsafe accessor code

```csharp
public partial class FruitMapper
{
    private partial global::FruitDto ToDto(global::Fruit source)
    {
        var target = new global::FruitDto();
        target.GetIsSeeded1() = source.GetIsSeeded();
        target.Name = source.Name;
        target.SetSweetness(source.GetSweetness());
        return target;
    }
}

static file class UnsafeAccessor
{
    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_isSeeded")]
    public static extern ref bool GetSeeded(this global::Fruit target);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_isSeeded")]
    public static extern ref bool GetSeeded1(this global::FruitDto target);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "get_Sweetness")]
    public static extern int GetSweetness(this global::Fruit source);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_Sweetness")]
    public static extern void SetSweetness(this global::FruitDto target, int value);
}
```

Here Mapperly generates a file scoped class containing extension method for each internal member for both the source and target. Mapperly then uses the extension methods to get and set the members. Note that this uses zero reflection and is as performant as using an ordinary property or field.

## Controlling member accessibility

In addition to mapping inaccessible members, `MemberVisbility` can be used to control which members are mapped, depending on their accessibility modifier. For instance `MemberVisibility.Private | MemberVisibility.Protected` will cause mapperly to only map private and protected members, generating an unsafe accessor if needed.

```csharp
public class Car
{
    private int _cost;

    public string Name { get; set; }

    protected string Engine { get; set; }
}

// highlight-start
[Mapper(IncludedMembers = MemberVisibility.Private | MemberVisibility.Protected)]
// highlight-end
public partial class CarMapper
{
    public partial CarDto ToDto(Car source);
}
```

## Generated member visibility code

```csharp
public partial class CarMapper
{
    private partial global::CarDto ToDto(global::Car source)
    {
        var target = new global::CarDto();
        target.GetCost1() = source.GetCost();
        target.SetEngine(source.GetEngine());
        return target;
    }
}

static file class UnsafeAccessor
{
    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_cost")]
    public static extern ref int GetCost(this global::Car target);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_cost")]
    public static extern ref int GetSeeded1(this global::CarDto target);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "get_Engine")]
    public static extern string GetEngine(this global::Car source);

    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = "set_Engine")]
    public static extern void SetEngine(this global::CarDto target, string value);
}
```
