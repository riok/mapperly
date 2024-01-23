using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private const string UnsafeAccessorName = "global::System.Runtime.CompilerServices.UnsafeAccessor";
    private const string UnsafeAccessorKindName = "global::System.Runtime.CompilerServices.UnsafeAccessorKind";
    private const string UnsafeAccessorNameArgument = "Name";

    private static readonly IdentifierNameSyntax _unsafeAccessorKindName = IdentifierName(UnsafeAccessorKindName);

    public SyntaxList<AttributeListSyntax> AttributeList(string name, params ExpressionSyntax[] arguments)
    {
        var args = CommaSeparatedList(arguments.Select(AttributeArgument));

        var attribute = Attribute(IdentifierName(name)).WithArgumentList(AttributeArgumentList(args));

        return SingletonList(SyntaxFactory.AttributeList(SingletonSeparatedList(attribute)).AddTrailingLineFeed(Indentation));
    }

    public SyntaxList<AttributeListSyntax> UnsafeAccessorAttributeList(UnsafeAccessorType type, string name)
    {
        var unsafeAccessType = type switch
        {
            UnsafeAccessorType.Field => "Field",
            UnsafeAccessorType.Method => "Method",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(UnsafeAccessorType)}"),
        };

        return AttributeList(
            UnsafeAccessorName,
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, _unsafeAccessorKindName, IdentifierName(unsafeAccessType)),
            Assignment(IdentifierName(UnsafeAccessorNameArgument), StringLiteral(name))
        );
    }

    public enum UnsafeAccessorType
    {
        Method,
        Field
    }
}
