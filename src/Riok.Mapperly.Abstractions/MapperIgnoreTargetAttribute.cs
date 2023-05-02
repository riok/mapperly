namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a target property from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapperIgnoreTargetAttribute : Attribute
{
    /// <summary>
    /// Ignores the specified target property from the mapping.
    /// </summary>
    /// <param name="target">The name of the target property to ignore. The use of `nameof()` is encouraged.</param>
    public MapperIgnoreTargetAttribute(string target)
    {
        Target = target;
    }

    /// <summary>
    /// Gets the target property name which should be ignored from the mapping.
    /// </summary>
    public string Target { get; }
}
