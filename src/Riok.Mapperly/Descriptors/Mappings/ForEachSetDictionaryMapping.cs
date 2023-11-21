using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach dictionary mapping which works by looping through the source,
/// mapping each element and setting it to the target collection.
/// </summary>
public class ForEachSetDictionaryMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    INewInstanceMapping keyMapping,
    INewInstanceMapping valueMapping,
    bool sourceHasCount,
    ITypeSymbol? typeToInstantiate = null,
    ObjectFactory? objectFactory = null,
    INamedTypeSymbol? explicitCast = null,
    EnsureCapacityInfo? ensureCapacity = null
)
    : ExistingTargetMappingMethodWrapper(
        new ForEachSetDictionaryExistingTargetMapping(sourceType, targetType, keyMapping, valueMapping, explicitCast, ensureCapacity)
    )
{
    private const string CountPropertyName = nameof(IDictionary<object, object>.Count);

    private readonly ITypeSymbol _typeToInstantiate = typeToInstantiate ?? targetType;

    protected override ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx)
    {
        if (objectFactory != null)
            return objectFactory.CreateType(SourceType, _typeToInstantiate, ctx.Source);

        if (sourceHasCount)
            return CreateInstance(_typeToInstantiate, MemberAccess(ctx.Source, CountPropertyName));

        return CreateInstance(_typeToInstantiate);
    }
}
