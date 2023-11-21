using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping generated from invoking <see cref="INewInstanceMapping.Build"/> on a source value.
/// <code>
/// target = Map(source);
/// </code>
/// </summary>
public class DelegateMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
    : NewInstanceMapping(sourceType, targetType)
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => delegateMapping.Build(ctx);
}
