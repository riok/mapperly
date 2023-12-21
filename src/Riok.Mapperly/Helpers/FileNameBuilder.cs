using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Helpers;

public class FileNameBuilder
{
    private const string GeneratedFileSuffix = ".g.cs";

    private readonly UniqueNameBuilder _uniqueNameBuilder = new();

    internal string Build(MapperDescriptor mapper) => _uniqueNameBuilder.New(mapper.Name) + GeneratedFileSuffix;
}
