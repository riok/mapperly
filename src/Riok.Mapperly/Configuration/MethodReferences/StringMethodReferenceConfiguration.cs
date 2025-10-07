using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration.MethodReferences;

public class StringMethodReferenceConfiguration(string name, string? targetName, string fullName) : IMethodReferenceConfiguration
{
    private bool _targetResolved;
    private INamedTypeSymbol? _targetType;
    private ISymbol? _targetMember;

    public string Name => name;

    public string FullName => fullName;

    public INamedTypeSymbol? GetTargetType(SimpleMappingBuilderContext ctx)
    {
        if (targetName is null)
        {
            return ctx.MapperDeclaration.Symbol;
        }

        EnsureTargetResolved(ctx);
        return _targetType;
    }

    public string? GetTargetName(SimpleMappingBuilderContext ctx)
    {
        if (targetName is null)
        {
            return null;
        }

        EnsureTargetResolved(ctx);
        if (_targetMember is not null)
        {
            return _targetMember.Name;
        }

        if (_targetType is not null)
        {
            return _targetType.FullyQualifiedIdentifierName();
        }

        return targetName;
    }

    public bool IsExternal => targetName != null;

    public override string ToString() => FullName;

    private void EnsureTargetResolved(SimpleMappingBuilderContext ctx)
    {
        if (_targetResolved)
        {
            return;
        }

        if (targetName is null)
        {
            _targetType = ctx.MapperDeclaration.Symbol;
            _targetResolved = true;
            return;
        }

        if (targetName.Contains('.', StringComparison.Ordinal))
        {
            // Fully qualified name, return what we found or null.
            _targetType = ctx.SymbolAccessor.GetTypeByMetadataName(targetName);
            _targetResolved = true;
            return;
        }

        // Either a field or property name, or a type in the global namespace.
        var targetMember = ctx
            .SymbolAccessor.GetAllMembers(ctx.MapperDeclaration.Symbol)
            .Where(m => m is IFieldSymbol or IPropertySymbol)
            .FirstOrDefault(m => string.Equals(m.Name, targetName, StringComparison.Ordinal));

        _targetType = targetMember switch
        {
            IFieldSymbol fieldSymbol => _targetType = fieldSymbol.Type as INamedTypeSymbol,
            IPropertySymbol propertySymbol => _targetType = propertySymbol.Type as INamedTypeSymbol,
            _ => ctx.SymbolAccessor.GetTypeByMetadataName(targetName),
        };

        _targetMember = targetMember;
        _targetResolved = true;
    }
}
