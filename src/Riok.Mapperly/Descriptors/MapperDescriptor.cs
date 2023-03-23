using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class MapperDescriptor
{
    private readonly List<MethodMapping> _methodMappings = new();

    public MapperDescriptor(ClassDeclarationSyntax syntax, INamedTypeSymbol symbol, UniqueNameBuilder nameBuilder)
    {
        Syntax = syntax;
        Symbol = symbol;
        NameBuilder = nameBuilder;

        if (!Symbol.ContainingNamespace.IsGlobalNamespace)
        {
            Namespace = Symbol.ContainingNamespace.ToDisplayString();
        }
    }

    public string? Namespace { get; }

    public ClassDeclarationSyntax Syntax { get; }

    public INamedTypeSymbol Symbol { get; }

    public UniqueNameBuilder NameBuilder { get; }

    public IReadOnlyCollection<MethodMapping> MethodTypeMappings => _methodMappings;

    public void AddTypeMapping(MethodMapping mapping)
        => _methodMappings.Add(mapping);
}
