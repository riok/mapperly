using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilder;

public static class UserMethodMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedNewInstanceMethodMapping mapping)
    {
        var delegateMapping = mapping.CallableByOtherMappings
            ? ctx.BuildDelegateMapping(mapping.SourceType, mapping.TargetType)
            : ctx.BuildMappingWithUserSymbol(mapping.SourceType, mapping.TargetType);
        if (delegateMapping != null)
        {
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.CouldNotCreateMapping,
            mapping.SourceType,
            mapping.TargetType);
    }
}
