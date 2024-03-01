using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Ignores a member from the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperIgnoreMemberAttribute : Attribute;
