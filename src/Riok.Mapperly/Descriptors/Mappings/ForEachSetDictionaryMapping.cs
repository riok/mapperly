using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach dictionary mapping which works by looping through the source,
/// mapping each element and setting it to the target collection.
/// </summary>
public class ForEachSetDictionaryMapping : ExistingTargetMappingMethodWrapper
{
    private const string CountPropertyName = nameof(IDictionary<object, object>.Count);

    private readonly bool _sourceHasCount;
    private readonly ObjectFactory? _objectFactory;
    private readonly ITypeSymbol _typeToInstantiate;

    public ForEachSetDictionaryMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping keyMapping,
        ITypeMapping valueMapping,
        bool sourceHasCount,
        ITypeSymbol? typeToInstantiate = null,
        ObjectFactory? objectFactory = null,
        INamedTypeSymbol? explicitCast = null,
        EnsureCapacity? ensureCapacity = null
    )
        : base(
            new ForEachSetDictionaryExistingTargetMapping(sourceType, targetType, keyMapping, valueMapping, explicitCast, ensureCapacity)
        )
    {
        _sourceHasCount = sourceHasCount;
        _objectFactory = objectFactory;
        _typeToInstantiate = typeToInstantiate ?? targetType;
    }

    protected override ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx)
    {
        if (_objectFactory != null)
            return _objectFactory.CreateType(SourceType, _typeToInstantiate, ctx.Source);

        if (_sourceHasCount)
            return CreateInstance(_typeToInstantiate, MemberAccess(ctx.Source, CountPropertyName));

        return CreateInstance(_typeToInstantiate);
    }
}
