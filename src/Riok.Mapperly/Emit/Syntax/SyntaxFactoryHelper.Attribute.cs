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

    public AttributeListSyntax UnsafeAccessorAttribute(UnsafeAccessorType type, string? name = null)
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
            return Attribute(UnsafeAccessorName, kind);

        return Attribute(UnsafeAccessorName, kind, Assignment(IdentifierName(UnsafeAccessorNameArgument), StringLiteral(name)));
    }

    private AttributeListSyntax Attribute(string name, params ExpressionSyntax[] arguments)
    {
        var args = CommaSeparatedList(arguments.Select(AttributeArgument));
        var attribute = SyntaxFactory.Attribute(IdentifierName(name));
        if (args.Count > 0)
        {
            attribute = attribute.WithArgumentList(AttributeArgumentList(args));
        }

        return AttributeList(SingletonSeparatedList(attribute)).AddTrailingLineFeed(Indentation);
    }

    public enum UnsafeAccessorType
    {
        Method,
        Field,
        Constructor,
    }
}
