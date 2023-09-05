using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Templates;

internal static class TemplateResolver
{
    internal static void AddRequiredTemplates(MapperAttribute mapperConfiguration, MappingCollection mappings, MapperDescriptor descriptor)
    {
        AddPreserveReferenceHandlerIfNeeded(mapperConfiguration, mappings, descriptor);
    }

    private static void AddPreserveReferenceHandlerIfNeeded(
        MapperAttribute mapperConfiguration,
        MappingCollection mappings,
        MapperDescriptor descriptor
    )
    {
        // if reference handling is enabled and any user defined method mapping
        // does not have a reference handling parameter,
        // emit the preserve reference handler template which gets instantiated as reference handler.
        if (
            mapperConfiguration.UseReferenceHandling
            && mappings.UserMappings.OfType<MethodMapping>().Any(x => !x.HasReferenceHandlingParameter())
        )
        {
            descriptor.AddRequiredTemplate(TemplateReference.PreserveReferenceHandler);
        }
    }
}
