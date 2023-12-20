using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Marks the constructor to be used when type gets activated by Mapperly.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperConstructorAttribute : Attribute;
