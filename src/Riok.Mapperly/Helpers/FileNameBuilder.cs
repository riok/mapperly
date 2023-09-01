using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Templates;

namespace Riok.Mapperly.Helpers;

public class FileNameBuilder
{
    private const string GeneratedFileSuffix = ".g.cs";
    private const string TemplatesGeneratedFileNamePrefix = "MapperlyInternal.";

    private readonly UniqueNameBuilder _uniqueNameBuilder = new();

    internal string Build(MapperDescriptor mapper) => _uniqueNameBuilder.New(mapper.Name) + GeneratedFileSuffix;

    internal static string BuildForTemplate(TemplateReference reference) =>
        TemplatesGeneratedFileNamePrefix + reference + GeneratedFileSuffix;
}
