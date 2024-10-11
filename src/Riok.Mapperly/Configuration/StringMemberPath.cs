using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public readonly record struct StringMemberPath(ImmutableEquatableArray<string> Path) : IMemberPathConfiguration
{
    public static readonly StringMemberPath Empty = new(ImmutableEquatableArray<string>.Empty);

    public StringMemberPath(IEnumerable<string> path)
        : this(path.ToImmutableEquatableArray()) { }

    public string RootName => Path[0];
    public string FullName => string.Join(MemberPathConstants.MemberAccessSeparatorString, Path);
    public int PathCount => Path.Count;

    public override string ToString() => FullName;

    public StringMemberPath SkipRoot() => new(Path.Skip(1).ToImmutableEquatableArray());
}
