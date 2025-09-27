using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration.MethodReferences;

/// <summary>
/// A reference to an external static method.
/// </summary>
/// <param name="Name">The method name.</param>
/// <param name="TargetType">The type containing the method.</param>
public record ExternalStaticMethodReferenceConfiguration(string Name, INamedTypeSymbol TargetType) : IMethodReferenceConfiguration
{
    public INamedTypeSymbol GetTargetType(SimpleMappingBuilderContext ctx) => TargetType;

    public string TargetName { get; } = TargetType.FullyQualifiedIdentifierName();

    public string FullName => $"{TargetName}.{Name}";

    public bool IsExternal => true;

    public override string ToString() => FullName;
}
