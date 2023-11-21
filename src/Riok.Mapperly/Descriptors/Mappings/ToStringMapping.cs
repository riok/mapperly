using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by invoking
/// the <see cref="object.ToString"/> instance method on the source object.
/// <code>
/// target = source.ToString();
/// </code>
/// </summary>
public class ToStringMapping(ITypeSymbol sourceType, ITypeSymbol targetType, string? stringFormat = null)
    : SourceObjectMethodMapping(sourceType, targetType, nameof(ToString))
{
    protected override IEnumerable<ExpressionSyntax> BuildArguments(TypeMappingBuildContext ctx)
    {
        if (stringFormat == null)
            yield break;

        yield return StringLiteral(stringFormat);
        yield return NullLiteral();
    }
}
