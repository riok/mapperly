using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Mapping body builder for user defined methods.
/// </summary>
public static class UserMethodMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedExistingTargetMethodMapping mapping)
    {
        // UserDefinedExistingTargetMethodMapping handles null already
        var delegateMapping = ctx.BuildExistingTargetMappingWithUserSymbol(
            mapping.SourceType.NonNullable(),
            mapping.TargetType.NonNullable()
        );

        if (delegateMapping != null)
        {
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, mapping.SourceType, mapping.TargetType);
    }

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

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, mapping.SourceType, mapping.TargetType);
    }
}
