using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration.MethodReferences;

public record ExternalInstanceMethodReferenceConfiguration(string Name, ISymbol TargetMember, INamedTypeSymbol TargetType)
    : IMethodReferenceConfiguration
{
    public INamedTypeSymbol GetTargetType(SimpleMappingBuilderContext ctx) => TargetType;

    public string TargetName => TargetMember.Name;

    public string FullName => $"{TargetMember.Name}.{Name}";

    public bool IsExternal => true;

    public override string ToString() => FullName;
}
