namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a method as an object factory.
/// An object factory can be used to instantiate or resolve target objects.
/// An object factory method needs to a parameterless method with a non-void return type.
/// It can be generic with constraints.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ObjectFactoryAttribute : Attribute
{
}
