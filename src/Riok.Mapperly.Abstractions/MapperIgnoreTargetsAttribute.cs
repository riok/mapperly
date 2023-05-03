namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores multiple target properties from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapperIgnoreTargetsAttribute : Attribute
{
    /// <summary>
    /// Ignores multiple target properties from the mapping.
    /// </summary>
    /// <param name="targets">Target property names to ignore. The use of `nameof()` is encouraged.</param>
    public MapperIgnoreTargetsAttribute(params string[] targets)
    {
        Targets = targets;
    }

    /// <summary>
    /// Gets the names of target properties which should be ignored from the mapping.
    /// </summary>
    public IEnumerable<string> Targets { get; }
}
