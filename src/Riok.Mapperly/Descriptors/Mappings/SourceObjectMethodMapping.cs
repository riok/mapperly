using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping which works by invoking an instance method on the source object
/// and then mapping the result with an optional mapper.
/// <code>
/// target = source.ToArray();
/// target = Map(source.ToArray());
/// </code>
/// </summary>
public class SourceObjectMethodMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    string methodName,
    INewInstanceMapping? delegateMapping = null
) : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        var sourceExpression = Invocation(MemberAccess(ctx.Source, methodName), BuildArguments(ctx).WhereNotNull().ToArray());
        return delegateMapping == null ? sourceExpression : delegateMapping.Build(ctx.WithSource(sourceExpression));
    }

    protected virtual IEnumerable<ExpressionSyntax?> BuildArguments(TypeMappingBuildContext ctx) => Enumerable.Empty<ExpressionSyntax?>();
}
