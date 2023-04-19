namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a method as an object factory.
/// An object factory can be used to instantiate or resolve target objects.
/// An object factory method needs to be a method with a non-void return type.
/// It can be generic with constraints and can have one or none parameters.
/// If the object factory has a parameter, the source object is provided as an argument.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ObjectFactoryAttribute : Attribute { }
