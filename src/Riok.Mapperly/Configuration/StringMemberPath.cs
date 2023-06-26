namespace Riok.Mapperly.Configuration;

public record StringMemberPath(IReadOnlyCollection<string> Path)
{
    public const char PropertyAccessSeparator = '.';
    private const string PropertyAccessSeparatorString = ".";

    public string FullName => string.Join(PropertyAccessSeparatorString, Path);

    public override string ToString() => FullName;
}
