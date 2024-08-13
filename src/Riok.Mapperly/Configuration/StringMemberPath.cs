using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public readonly record struct StringMemberPath(ImmutableEquatableArray<string> Path)
{
    public static readonly StringMemberPath Empty = new(ImmutableEquatableArray<string>.Empty);

    public StringMemberPath(IEnumerable<string> path)
        : this(path.ToImmutableEquatableArray()) { }

    public const char MemberAccessSeparator = '.';
    private const string MemberAccessSeparatorString = ".";

    public string FullName => string.Join(MemberAccessSeparatorString, Path);

    public override string ToString() => FullName;

    public StringMemberPath SkipRoot() => new(Path.Skip(1).ToImmutableEquatableArray());
}
