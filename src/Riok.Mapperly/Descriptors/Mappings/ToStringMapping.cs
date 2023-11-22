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
/// </summary>
public class ToStringMapping(ITypeSymbol sourceType, ITypeSymbol targetType, string? stringFormat = null, string? formatProviderName = null)
    : SourceObjectMethodMapping(sourceType, targetType, nameof(ToString))
{
    protected override IEnumerable<ExpressionSyntax> BuildArguments(TypeMappingBuildContext ctx)
    {
        if (stringFormat == null && formatProviderName == null)
            yield break;

        yield return stringFormat == null ? NullLiteral() : StringLiteral(stringFormat);
        yield return formatProviderName == null ? NullLiteral() : IdentifierName(formatProviderName);
    }
}
