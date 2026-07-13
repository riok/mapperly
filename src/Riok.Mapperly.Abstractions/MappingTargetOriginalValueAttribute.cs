using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a parameter of a user-implemented mapping method as receiving the current value
/// of the destination member being mapped to.
/// This allows the method to use the existing destination value (e.g. as a fallback).
/// </summary>
/// <example>
/// <code>
/// private static int? FromOptional(Optional&lt;int?&gt; source, [MappingTargetOriginalValue] int? original)
///     =&gt; source.HasValue ? source.Value : original;
/// </code>
/// Mapperly will generate: <c>destination.Age = FromOptional(source.Age, destination.Age);</c>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MappingTargetOriginalValueAttribute : Attribute;
