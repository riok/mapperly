using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration.MethodReferences;

public interface IMethodReferenceConfiguration
{
    INamedTypeSymbol? GetTargetType(SimpleMappingBuilderContext ctx);

    string Name { get; }

    string? TargetName { get; }

    string FullName { get; }

    bool IsExternal { get; }
}
