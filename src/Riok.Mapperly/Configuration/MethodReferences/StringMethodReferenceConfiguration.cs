using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration.MethodReferences;

public record StringMethodReferenceConfiguration(string Name, string? TargetName, string FullName) : IMethodReferenceConfiguration
{
    private INamedTypeSymbol? _targetType;

    public INamedTypeSymbol? GetTargetType(SimpleMappingBuilderContext ctx)
    {
        if (TargetName is null)
        {
            return ctx.MapperDeclaration.Symbol;
        }

        if (_targetType is not null)
        {
            return _targetType;
        }

        if (TargetName.Contains('.', StringComparison.Ordinal))
        {
            // Fully qualified name, return what we found or null.
            _targetType = ctx.SymbolAccessor.GetTypeByMetadataName(TargetName);
            return _targetType;
        }

        // Either a field or property name, or a type in the global namespace.
        var targetSymbol = ctx
            .SymbolAccessor.GetAllMembers(ctx.MapperDeclaration.Symbol)
            .Where(m => m is IFieldSymbol or IPropertySymbol)
            .FirstOrDefault(m => string.Equals(m.Name, TargetName, StringComparison.Ordinal));

        _targetType = targetSymbol switch
        {
            IFieldSymbol fieldSymbol => _targetType = fieldSymbol.Type as INamedTypeSymbol,
            IPropertySymbol propertySymbol => _targetType = propertySymbol.Type as INamedTypeSymbol,
            _ => ctx.SymbolAccessor.GetTypeByMetadataName(TargetName),
        };

        return _targetType;
    }

    public bool IsExternal => TargetName != null;

    public override string ToString() => FullName;
}
