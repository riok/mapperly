using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public record class MethodReferenceConfiguration(string Name, INamedTypeSymbol? TargetType = null, ISymbol? TargetMember = null)
{
    public string? TargetTypeName { get; } = TargetType?.FullyQualifiedIdentifierName();

    public string FullName { get; } = TargetType is null ? Name : $"{TargetType.FullyQualifiedIdentifierName()}.{Name}";

    public override string ToString() => FullName;
}
