using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a member or method from the mapping.
/// </summary>
/// <remarks>
/// When applied to a method, prevents the method from being used as a mapping method.
/// This is useful for excluding specific static or instance methods from being used as mappers.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperIgnoreAttribute : Attribute;
