using System.Diagnostics;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Considers all static mapping methods provided by the type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class UseStaticMapperAttribute : Attribute
{
    /// <summary>
    /// Considers all static mapping methods provided by the <paramref name="mapperType"/>.
    /// </summary>
    /// <param name="mapperType">The type of which mapping methods will be included.</param>
    public UseStaticMapperAttribute(Type mapperType) { }

    /// <summary>
    /// If <c>true</c>, the mapping methods of the referenced mapper are not inlined into the expression trees
    /// of the queryable projection and expression mappings of the referencing mapper but invoked instead.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool NoExpressionInlining { get; set; }
}

/// <summary>
/// Considers all static mapping methods provided by the generic type.
/// </summary>
/// <typeparam name="T">The type of which mapping methods will be included.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]
public sealed class UseStaticMapperAttribute<T> : Attribute
{
    /// <summary>
    /// If <c>true</c>, the mapping methods of the referenced mapper are not inlined into the expression trees
    /// of the queryable projection and expression mappings of the referencing mapper but invoked instead.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool NoExpressionInlining { get; set; }
}
