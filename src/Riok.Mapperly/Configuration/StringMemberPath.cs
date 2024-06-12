using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public record StringMemberPath(ImmutableEquatableArray<string> Path)
{
    public static readonly StringMemberPath Empty = new(ImmutableEquatableArray<string>.Empty);

    public const char MemberAccessSeparator = '.';
    private const string MemberAccessSeparatorString = ".";

    public string FullName => string.Join(MemberAccessSeparatorString, Path);

    public override string ToString() => FullName;
}
