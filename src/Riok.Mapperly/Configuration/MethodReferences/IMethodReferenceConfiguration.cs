namespace Riok.Mapperly.Configuration.MethodReferences;

public interface IMethodReferenceConfiguration
{
    string Name { get; }

    string FullName { get; }

    bool IsExternal { get; }
}
