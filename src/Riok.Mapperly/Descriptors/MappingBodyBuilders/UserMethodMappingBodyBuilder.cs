using Riok.Mapperly.Descriptors.Mappings;
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

        // the delegate mapping is not embedded
        // and is therefore reusable if there are no additional parameters
        // if embedded, only the original mapping is callable by others
        if (mapping is { InternalReferenceHandlingEnabled: true, AdditionalSourceParameters.Count: 0 })
        {
            options |= MappingBuildingOptions.MarkAsReusable;
        }

        var delegateMapping = ctx.BuildMapping(new TypeMappingKey(mapping), options);
        if (delegateMapping != null)
        {
            // Prevent DI early-return recursion on the root mapping by disabling it on the delegate if applicable.
            if (delegateMapping is NewInstanceObjectMemberMethodMapping m)
            {
                m.DisableDiEarlyReturn = true;
            }
            mapping.SetDelegateMapping(delegateMapping);
            return;
        }

        ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, mapping.SourceType, mapping.TargetType);
    }
}
