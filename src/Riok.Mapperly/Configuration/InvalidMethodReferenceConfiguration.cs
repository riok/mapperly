namespace Riok.Mapperly.Configuration;

public record InvalidMethodReferenceConfiguration(string Name, string? Target) : MethodReferenceConfiguration(Name)
{
    public override string FullName => Target is not null ? $"{Target}.{Name}" : Name;

    public override bool IsExternal => true;

    public override string ToString() => FullName;
}
