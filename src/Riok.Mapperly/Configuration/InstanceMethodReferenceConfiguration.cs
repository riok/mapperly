using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration;

public record InstanceMethodReferenceConfiguration(string Name, ISymbol TargetMember, INamedTypeSymbol TargetType)
    : ExternalMethodReferenceConfiguration(Name, TargetType)
{
    public override string TargetName { get; } = TargetMember.Name;
    public override string FullName => $"{TargetName}.{Name}";
}
