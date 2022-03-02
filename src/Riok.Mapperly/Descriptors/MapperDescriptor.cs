using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{
    private const string FileNameSuffix = ".g.cs";

    private readonly List<TypeMapping> _mappings = new();

    public MapperDescriptor(string name, ClassDeclarationSyntax syntax)
    {
        FileName = name + FileNameSuffix;
        Syntax = syntax;
    }

    public string? Namespace { get; set; }

    public string FileName { get; }

    public ClassDeclarationSyntax Syntax { get; }

    public IEnumerable<MethodMapping> MethodTypeMappings
        => _mappings.OfType<MethodMapping>();

    public void AddTypeMapping(TypeMapping mapping)
        => _mappings.Add(mapping);
}
