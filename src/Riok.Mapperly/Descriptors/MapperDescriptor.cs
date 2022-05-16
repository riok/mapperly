using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{
    private const string FileNameSuffix = ".g.cs";

    private readonly List<TypeMapping> _mappings = new();

    public MapperDescriptor(string name, ClassDeclarationSyntax syntax, bool isStatic)
    {
        FileName = name + FileNameSuffix;
        Syntax = syntax;
        IsStatic = isStatic;
    }

    public string? Namespace { get; set; }

    public string FileName { get; }

    public ClassDeclarationSyntax Syntax { get; }

    public bool IsStatic { get; }

    public IEnumerable<MethodMapping> MethodTypeMappings
        => _mappings.OfType<MethodMapping>();

    public void AddTypeMapping(TypeMapping mapping)
        => _mappings.Add(mapping);
}
