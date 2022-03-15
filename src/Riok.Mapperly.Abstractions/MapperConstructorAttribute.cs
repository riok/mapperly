namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks the constructor to be used when type gets activated by Mapperly.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class MapperConstructorAttribute : Attribute
{
}
