using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration.MethodReferences;

public record ExternalInstanceMethodReferenceConfiguration(string Name, ISymbol TargetMember, INamedTypeSymbol TargetType)
    : IExternalMethodReferenceConfiguration
{
    public string TargetName => TargetMember.Name;
    public string FullName => $"{TargetMember.Name}.{Name}";
    public bool IsExternal { get; } = true;
}
