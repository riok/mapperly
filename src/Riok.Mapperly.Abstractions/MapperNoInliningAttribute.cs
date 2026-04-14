using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Prevents a mapping method from being inlined into expression trees for queryable projection mappings.
/// When applied, the method call is preserved as-is instead of being rebuilt in expression context.
/// This is useful when inlining causes issues such as false enum mapping diagnostics
/// due to expression tree limitations.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperNoInliningAttribute : Attribute;
