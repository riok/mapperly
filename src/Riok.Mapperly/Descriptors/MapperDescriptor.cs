using System.Text;
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
        Name = BuildName(symbol);

        if (!Symbol.ContainingNamespace.IsGlobalNamespace)
        {
            Namespace = Symbol.ContainingNamespace.ToDisplayString();
        }
    }

    public string Name { get; }

    public string? Namespace { get; }

    public ClassDeclarationSyntax Syntax { get; }

    public INamedTypeSymbol Symbol { get; }

    public UniqueNameBuilder NameBuilder { get; }

    public IReadOnlyCollection<MethodMapping> MethodTypeMappings => _methodMappings;

    public void AddTypeMapping(MethodMapping mapping)
        => _methodMappings.Add(mapping);

    private string BuildName(INamedTypeSymbol symbol)
    {
        if (symbol.ContainingType == null)
            return symbol.Name;

        var sb = new StringBuilder(symbol.Name);
        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            sb.Insert(0, '.').Insert(0, containingType.Name);
            containingType = containingType.ContainingType;
        }

        return sb.ToString();
    }
}

