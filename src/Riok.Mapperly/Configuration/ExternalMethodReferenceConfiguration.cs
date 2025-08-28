using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public abstract record ExternalMethodReferenceConfiguration(string Name, INamedTypeSymbol TargetType) : MethodReferenceConfiguration(Name)
{
    public abstract string TargetName { get; }
    public override bool IsExternal => true;
}
