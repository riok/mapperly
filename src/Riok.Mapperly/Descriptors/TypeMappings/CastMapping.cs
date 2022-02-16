using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a cast mapping.
/// </summary>
public class CastMapping : TypeMapping
{
    private readonly TypeMapping? _delegateMapping;

    public CastMapping(ITypeSymbol sourceType, ITypeSymbol targetType, TypeMapping? delegateMapping = null)
        : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        return CastExpression(IdentifierName(TargetType.ToDisplayString()), _delegateMapping != null ? _delegateMapping.Build(source) : source);
    }
}
