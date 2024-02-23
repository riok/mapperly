namespace Riok.Mapperly.Configuration;

public record StringMemberPath(IReadOnlyCollection<string> Path)
{
    public const char MemberAccessSeparator = '.';
    private const string MemberAccessSeparatorString = ".";

    public string FullName => string.Join(MemberAccessSeparatorString, Path);

    public override string ToString() => FullName;
}
