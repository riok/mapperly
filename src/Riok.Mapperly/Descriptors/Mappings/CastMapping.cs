using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a cast mapping.
/// </summary>
public class CastMapping : TypeMapping
{
    private readonly ITypeMapping? _delegateMapping;

    public CastMapping(ITypeSymbol sourceType, ITypeSymbol targetType, ITypeMapping? delegateMapping = null)
        : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var objToCast = _delegateMapping != null
            ? _delegateMapping.Build(ctx)
            : ctx.Source;
        return CastExpression(TargetType.GetFullyQualifiedTypeSyntax(), objToCast);
    }
}
