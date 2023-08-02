namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Specifies derived type mappings for which a mapping should be generated.
/// A type switch is implemented over the source object and the provided source types.
/// Each source type has to be unique but multiple source types can be mapped to the same target type.
/// Each source type needs to extend or implement the parameter type of the mapping method.
/// Each target type needs to extend or implement the return type of the mapping method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapDerivedTypeAttribute : Attribute
{
    /// <summary>
    /// Registers a derived type mapping.
    /// </summary>
    /// <param name="sourceType">The derived source type.</param>
    /// <param name="targetType">The derived target type.</param>
    public MapDerivedTypeAttribute(Type sourceType, Type targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    /// <summary>
    /// Gets the source type of the derived type mapping.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the target type of the derived type mapping.
    /// </summary>
    public Type TargetType { get; }
}

/// <summary>
/// Specifies derived type mappings for which a mapping should be generated.
/// A type switch is implemented over the source object and the provided source types.
/// Each source type has to be unique but multiple source types can be mapped to the same target type.
/// Each source type needs to extend or implement the parameter type of the mapping method.
/// Each target type needs to extend or implement the return type of the mapping method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class MapDerivedTypeAttribute<TSource, TTarget> : Attribute { }
