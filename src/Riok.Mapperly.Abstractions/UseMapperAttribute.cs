using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Considers all accessible mapping methods provided by the type of this member.
/// Includes static and instance methods.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class UseMapperAttribute : Attribute
{
    /// <summary>
    /// If <c>true</c>, the mapping methods of the referenced mapper are not inlined into the expression trees
    /// of the queryable projection and expression mappings of the referencing mapper but invoked instead.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool NoExpressionInlining { get; set; }
}
