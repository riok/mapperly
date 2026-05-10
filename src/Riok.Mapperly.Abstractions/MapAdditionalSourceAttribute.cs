using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks a parameter as an additional source for mapping.
/// Properties from this parameter will be available for mapping to the destination.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapAdditionalSourceAttribute : Attribute;
