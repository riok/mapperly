using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.TypeMappings;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{
    internal const string InterfaceNamePrefix = "I";
    internal const string ImplClassNameSuffix = "Impl";
    private const string FileNameSuffix = ".g.cs";

    private readonly List<TypeMapping> _mappings = new();

    public MapperDescriptor(string baseName)
    {
        BaseName = baseName;
        Name = baseName + ImplClassNameSuffix;
    }

    public string? Namespace { get; set; }

    public string Name { get; set; }

    public string BaseName { get; }

    public string FileName => Name + FileNameSuffix;

    public bool IsAbstractClassDefinition { get; set; }

    public Accessibility Accessibility { get; set; } = Accessibility.Public;

    public string? InstanceName { get; set; }

    public IEnumerable<MethodMapping> MethodTypeMappings
        => _mappings.OfType<MethodMapping>();

    public void AddTypeMapping(TypeMapping mapping)
        => _mappings.Add(mapping);
}
