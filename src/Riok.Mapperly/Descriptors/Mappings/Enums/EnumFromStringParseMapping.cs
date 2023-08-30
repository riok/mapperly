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
public class EnumFromStringParseMapping : NewInstanceMapping
{
    private const string EnumClassName = "System.Enum";
    private const string ParseMethodName = "Parse";

    private readonly bool _genericParseMethodSupported;
    private readonly bool _ignoreCase;

    public EnumFromStringParseMapping(ITypeSymbol sourceType, ITypeSymbol targetType, bool genericParseMethodSupported, bool ignoreCase)
        : base(sourceType, targetType)
    {
        _genericParseMethodSupported = genericParseMethodSupported;
        _ignoreCase = ignoreCase;
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // System.Enum.Parse<TargetType>(source, ignoreCase)
        if (_genericParseMethodSupported)
        {
            return GenericInvocation(
                EnumClassName,
                ParseMethodName,
                new[] { FullyQualifiedIdentifier(TargetType) },
                ctx.Source,
                BooleanLiteral(_ignoreCase)
            );
        }

        // (TargetType)System.Enum.Parse(typeof(TargetType), source, ignoreCase)
        var enumParseInvocation = Invocation(
            MemberAccess(EnumClassName, ParseMethodName),
            TypeOfExpression(FullyQualifiedIdentifier(TargetType)),
            ctx.Source,
            BooleanLiteral(_ignoreCase)
        );
        return CastExpression(FullyQualifiedIdentifier(TargetType), enumParseInvocation);
    }
}
