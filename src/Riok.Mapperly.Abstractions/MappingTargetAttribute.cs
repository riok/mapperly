using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a given parameter as the mapping target.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MappingTargetAttribute : Attribute;
