using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a cast mapping.
/// </summary>
public class CastMapping : TypeMapping
{
    public CastMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        return CastExpression(IdentifierName(TargetType.ToDisplayString()), source);
    }
}
