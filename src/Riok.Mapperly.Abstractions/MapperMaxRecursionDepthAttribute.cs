using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the maximum recursion depth that an IQueryable mapping will use.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperMaxRecursionDepthAttribute : Attribute
{
    /// <summary>
    /// Defines the maximum recursion depth that an IQueryable mapping will use.
    /// </summary>
    /// <param name="maxRecursionDepth">The maximum recursion depth used when mapping IQueryable members.</param>
    public MapperMaxRecursionDepthAttribute(uint maxRecursionDepth)
    {
        MaxRecursionDepth = maxRecursionDepth;
    }

    /// <summary>
    /// The maximum recursion depth used when mapping IQueryable members.
    /// </summary>
    public uint MaxRecursionDepth { get; }
}
