using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private const string NotNullIfNotNullAttributeName = "global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull";

    public AttributeListSyntax ReturnNotNullIfNotNullAttribute(ExpressionSyntax source)
    {
        return Attribute(NotNullIfNotNullAttributeName, ParameterNameOfOrStringLiteral(source))
            .WithTarget(AttributeTargetSpecifier(Token(SyntaxKind.ReturnKeyword)).AddTrailingSpace());
    }
}
