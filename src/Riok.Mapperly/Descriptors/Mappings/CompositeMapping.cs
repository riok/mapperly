using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Invokes a named mapping with optional input and output conversions.
/// <code>
/// OutputMapping(NamedMapping(InputMapping(source)));
/// </code>
/// </summary>
public class CompositeMapping(INewInstanceMapping namedMapping, INewInstanceMapping? inputMapping, INewInstanceMapping? outputMapping)
    : NewInstanceMapping(inputMapping?.SourceType ?? namedMapping.SourceType, outputMapping?.TargetType ?? namedMapping.TargetType)
{
    internal INewInstanceMapping NamedMapping { get; } = namedMapping;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // The input and output conversions do not receive the target value, even if they use [MappingTargetOriginalValue].
        // The target value is passed only to the named mapping.
        var conversionCtx = ctx.WithTargetValue(null);
        var source = inputMapping?.Build(conversionCtx) ?? ctx.Source;
        var result = NamedMapping.Build(ctx.WithSource(source));
        return outputMapping?.Build(conversionCtx.WithSource(result)) ?? result;
    }
}
