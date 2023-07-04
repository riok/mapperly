using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping generated from invoking <see cref="ITypeMapping.Build"/> on a source value.
/// <code>
/// target = Map(source);
/// </code>
/// </summary>
public class DelegateMapping : TypeMapping
{
    private readonly ITypeMapping _delegateMapping;

    public DelegateMapping(ITypeSymbol sourceType, ITypeSymbol targetType, ITypeMapping delegateMapping)
        : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => _delegateMapping.Build(ctx);
}
