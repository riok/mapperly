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

    public SyntaxList<AttributeListSyntax> UnsafeAccessorAttributeList(UnsafeAccessorType type, string? name = null)
    {
        var unsafeAccessType = type switch
        {
            UnsafeAccessorType.Field => "Field",
            UnsafeAccessorType.Method => "Method",
            UnsafeAccessorType.Constructor => "Constructor",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unknown {nameof(UnsafeAccessorType)}"),
        };

        var kind = MemberAccess(_unsafeAccessorKindName, IdentifierName(unsafeAccessType));
        if (name == null)
            return AttributeList(UnsafeAccessorName, kind);

        return AttributeList(UnsafeAccessorName, kind, Assignment(IdentifierName(UnsafeAccessorNameArgument), StringLiteral(name)));
    }

    public enum UnsafeAccessorType
    {
        Method,
        Field,
        Constructor,
    }
}
