using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{

    private readonly List<ITypeMapping> _mappings = new();

    public MapperDescriptor(ClassDeclarationSyntax syntax, bool isStatic)
    {
        Syntax = syntax;
        IsStatic = isStatic;
    }

    public string? Namespace { get; set; }

    public ClassDeclarationSyntax Syntax { get; }

    public bool IsStatic { get; }

    public IEnumerable<MethodMapping> MethodTypeMappings
        => _mappings.OfType<MethodMapping>();

    public void AddTypeMapping(ITypeMapping mapping)
        => _mappings.Add(mapping);
}
