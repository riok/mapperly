namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a property from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MapperIgnoreAttribute : Attribute
{
    /// <summary>
    /// Ignores the specified target property from the mapping.
    /// </summary>
    /// <param name="target">The name of the target property to ignore. The use of `nameof()` is encouraged.</param>
    public MapperIgnoreAttribute(string target)
    {
        Target = target;
    }

    /// <summary>
    /// Gets the property name which should be ignored from the mapping.
    /// </summary>
    public string Target { get; }
}
