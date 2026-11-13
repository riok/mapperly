using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Prevents the mapping method from being inlined into the expression trees
/// of queryable projection and expression mappings.
/// The method is invoked instead.
/// Applies to the mapping methods of the mapper, including inherited base class and interface methods,
/// but not to methods imported via <see cref="UseMapperAttribute"/> or <see cref="UseStaticMapperAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class MapperNoExpressionInliningAttribute : Attribute;
