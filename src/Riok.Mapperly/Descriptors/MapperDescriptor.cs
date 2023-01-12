using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{

    private readonly List<ITypeMapping> _mappings = new();

    public MapperDescriptor(ClassDeclarationSyntax syntax, INamedTypeSymbol symbol, UniqueNameBuilder nameBuilder)
    {
        Syntax = syntax;
        Symbol = symbol;
        NameBuilder = nameBuilder;
    }

    public string? Namespace { get; set; }

    public ClassDeclarationSyntax Syntax { get; }

    public INamedTypeSymbol Symbol { get; }

    public UniqueNameBuilder NameBuilder { get; }

    public IEnumerable<MethodMapping> MethodTypeMappings
        => _mappings.OfType<MethodMapping>();

    public void AddTypeMapping(ITypeMapping mapping)
        => _mappings.Add(mapping);
}
