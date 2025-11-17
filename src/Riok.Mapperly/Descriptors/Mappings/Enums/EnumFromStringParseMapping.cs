using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.Enums;

/// <summary>
/// Represents a mapping from a string to an enum.
/// Uses <see cref="Enum.Parse(Type, string, bool)"/>.
/// Less efficient than <see cref="EnumFromStringSwitchMapping"/>
/// but works in <see cref="System.Linq.Expressions.Expression{T}"/>.
/// </summary>
public class EnumFromStringParseMapping(ITypeSymbol sourceType, ITypeSymbol targetType, bool genericParseMethodSupported, bool ignoreCase)
    : NewInstanceMapping(sourceType, targetType)
{
    private const string EnumClassName = "global::System.Enum";
    private const string ParseMethodName = "Parse";

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // System.Enum.Parse<TargetType>(source, ignoreCase)
        if (genericParseMethodSupported)
        {
            return ctx.SyntaxFactory.GenericInvocation(
                EnumClassName,
                ParseMethodName,
                [FullyQualifiedIdentifier(TargetType)],
                ctx.Source,
                BooleanLiteral(ignoreCase)
            );
        }

        // (TargetType)System.Enum.Parse(typeof(TargetType), source, ignoreCase)
        var enumParseInvocation = ctx.SyntaxFactory.Invocation(
            MemberAccess(EnumClassName, ParseMethodName),
            TypeOfExpression(FullyQualifiedIdentifier(TargetType)),
            ctx.Source,
            BooleanLiteral(ignoreCase)
        );
        return CastExpression(FullyQualifiedIdentifier(TargetType), enumParseInvocation);
    }
}
