using Riok.Mapperly.Descriptors.Mappings.UserMappings;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders;

/// <summary>
/// Mapping body builder for user defined methods.
/// </summary>
public static class UserMethodMappingBodyBuilder
{
    public static void BuildMappingBody(MappingBuilderContext ctx, UserDefinedExistingTargetMethodMapping mapping)
    {
        // UserDefinedExistingTargetMethodMapping handles null already
        var delegateMapping = ctx.BuildExistingTargetMapping(
            new TypeMappingKey(mapping).NonNullable(),
            MappingBuildingOptions.KeepUserSymbol
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
        var options = MappingBuildingOptions.KeepUserSymbol;

        // this this mapping is not callable by others
        // the delegate mapping is probably callable by others
        // and therefore reusable
        if (!mapping.CallableByOtherMappings)
        {
            options |= MappingBuildingOptions.MarkAsReusable;
        }

        var delegateMapping = ctx.BuildMapping(new TypeMappingKey(mapping), options);
        if (delegateMapping != null)
        {
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, mapping.SourceType, mapping.TargetType);
    }
}
