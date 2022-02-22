using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping where the target type has the source as single ctor argument.
/// </summary>
public class CtorMapping : TypeMapping
{
    public CtorMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
    {
        var type = IdentifierName(TargetType.NonNullable().ToDisplayString());
        return ObjectCreationExpression(type).WithArgumentList(ArgumentList(source));
    }
}
