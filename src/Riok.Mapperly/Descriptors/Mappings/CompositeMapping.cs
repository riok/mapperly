using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Creates a composition of two mappings
/// <code>
/// OutputMapping(InnerMapping(source));
/// </code>
/// </summary>
public class CompositeMapping(INewInstanceMapping outerMapping, INewInstanceMapping innerMapping)
    : NewInstanceMapping(innerMapping.SourceType, outerMapping.TargetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => outerMapping.Build(ctx.WithSource(innerMapping.Build(ctx)));
}
