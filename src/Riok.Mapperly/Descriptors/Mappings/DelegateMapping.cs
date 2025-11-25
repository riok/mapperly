using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping generated from invoking <see cref="INewInstanceMapping.Build"/> on a source value.
/// This can be used to adjust the types of the mapping
/// (eg. use the same mapping to map from an interface as for an implementation for an interface,
/// List => ICollection)
/// <code>
/// target = Map(source);
/// </code>
/// </summary>
public class DelegateMapping(ITypeSymbol sourceType, ITypeSymbol targetType, INewInstanceMapping delegateMapping)
    : NewInstanceMapping(sourceType, targetType),
        IHasUsedParameters
{
    public override ExpressionSyntax Build(TypeMappingBuildContext ctx) => delegateMapping.Build(ctx);

    public IEnumerable<string> ExtractUsedParameters() => UsedParameterHelpers.ExtractUsedParameters(delegateMapping);
}
