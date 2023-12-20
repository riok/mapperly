using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Templates;

internal static class TemplateResolver
{
    internal static void AddRequiredTemplates(SimpleMappingBuilderContext ctx, MappingCollection mappings, MapperDescriptor descriptor)
    {
        AddPreserveReferenceHandlerIfNeeded(ctx, mappings, descriptor);
    }

    private static void AddPreserveReferenceHandlerIfNeeded(
        SimpleMappingBuilderContext ctx,
        MappingCollection mappings,
        MapperDescriptor descriptor
    )
    {
        // if reference handling is enabled and any user defined method mapping
        // does not have a reference handling parameter,
        // emit the preserve reference handler template which gets instantiated as reference handler.
        if (
            ctx.MapperConfiguration.UseReferenceHandling
            && mappings.UserMappings.OfType<MethodMapping>().Any(x => !x.HasReferenceHandlingParameter())
        )
        {
            AddTemplateIfTypeIsNotDefined(ctx, descriptor, TemplateReference.PreserveReferenceHandler);
        }
    }

    private static void AddTemplateIfTypeIsNotDefined(
        SimpleMappingBuilderContext ctx,
        MapperDescriptor descriptor,
        TemplateReference templateRef
    )
    {
        // this prevents collisions with InternalsVisibleTo to other assemblies which contain this type already.
        var type = TemplateReader.GetTypeName(templateRef);
        if (ctx.Types.TryGet(type) == null)
        {
            descriptor.AddRequiredTemplate(templateRef);
        }
    }
}
