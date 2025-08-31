using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Configuration.MethodReferences;

public interface IExternalMethodReferenceConfiguration : IMethodReferenceConfiguration
{
    INamedTypeSymbol TargetType { get; }

    string TargetName { get; }
}
