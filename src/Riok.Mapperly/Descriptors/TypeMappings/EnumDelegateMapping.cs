using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.TypeMappings;

public class EnumDelegateMapping : TypeMapping
{
    private readonly TypeMapping _delegateMapping;

    public EnumDelegateMapping(ITypeSymbol sourceType, ITypeSymbol targetType, TypeMapping delegateMapping) : base(sourceType, targetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => CastExpression(IdentifierName(TargetType.ToDisplayString()), _delegateMapping.Build(source));
}
