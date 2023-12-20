using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Considers all accessible mapping methods provided by the type of this member.
/// Includes static and instance methods.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class UseMapperAttribute : Attribute;
