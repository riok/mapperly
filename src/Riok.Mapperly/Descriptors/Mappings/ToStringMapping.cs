using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by invoking
/// the <see cref="object.ToString"/> instance method on the source object.
/// <code>
/// target = source.ToString();
/// </code>
/// <param name="simpleInvocation">
/// When <see langword="true"/>, <see langword="null"/> parameters are not emitted,
/// when <see langword="false"/>, <see langword="null"/> parameters are emitted as <see langword="null"/> literals.</param>
/// </summary>
public class ToStringMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    string? stringFormat = null,
    string? formatProviderName = null,
    bool simpleInvocation = true
) : SourceObjectMethodMapping(sourceType, targetType, nameof(ToString))
{
    protected override IEnumerable<ExpressionSyntax?> BuildArguments(TypeMappingBuildContext ctx)
    {
        yield return stringFormat != null ? StringLiteral(stringFormat)
        : simpleInvocation ? null
        : NullLiteral();
        yield return formatProviderName != null ? IdentifierName(formatProviderName)
        : simpleInvocation ? null
        : NullLiteral();
    }
}
