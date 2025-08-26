using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public record StaticMethodReferenceConfiguration(string Name, INamedTypeSymbol TargetType)
    : ExternalMethodReferenceConfiguration(Name, TargetType)
{
    public override string TargetName { get; } = TargetType.FullyQualifiedIdentifierName();
    public override string FullName => $"{TargetName}.{Name}";
}
