using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration.PropertyReferences;

/// <summary>
/// A configured member path consisting of resolved symbols.
/// </summary>
/// <param name="Path">The path.</param>
public record SymbolMemberPath(ImmutableEquatableArray<ISymbol> Path) : IMemberPathConfiguration
{
    private string? _fullName;

    public string RootName => Path[0].Name;
    public string FullName => _fullName ??= string.Join(MemberPathConstants.MemberAccessSeparatorString, MemberNames);
    public int PathCount => Path.Count;
    public IEnumerable<string> MemberNames => Path.Select(x => x.Name);

    public override string ToString() => FullName;

    public StringMemberPath ToStringMemberPath() => new(Path.Select(x => x.Name));
}
