namespace Riok.Mapperly.Configuration.MethodReferences;

public record InternalMethodReferenceConfiguration(string Name) : IMethodReferenceConfiguration
{
    public virtual string FullName => Name;

    public virtual bool IsExternal => false;

    public override string ToString() => FullName;
}
