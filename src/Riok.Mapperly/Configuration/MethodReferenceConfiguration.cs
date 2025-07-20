using Microsoft.CodeAnalysis;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public record class MethodReferenceConfiguration(string Name, ITypeSymbol? TargetType)
{
    public override string ToString() => TargetType is null ? $"{Name}()" : $"{TargetType.FullyQualifiedIdentifierName()}.{Name}()";
}
