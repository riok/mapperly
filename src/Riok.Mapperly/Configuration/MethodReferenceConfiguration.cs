namespace Riok.Mapperly.Configuration;

public record MethodReferenceConfiguration(string Name)
{
    public virtual string FullName => Name;

    public virtual bool IsExternal => true;

    public override string ToString() => FullName;
}
