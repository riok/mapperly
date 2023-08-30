using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableMapping : ExistingTargetMappingMethodWrapper
{
    private readonly ObjectFactory? _objectFactory;
    private readonly ITypeSymbol _typeToInstantiate;
    private readonly string? _countPropertyName;

    public ForEachAddEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INewInstanceMapping elementMapping,
        ObjectFactory? objectFactory,
        string insertMethodName,
        EnsureCapacityInfo? ensureCapacityBuilder = null
    )
        : base(
            new ForEachAddEnumerableExistingTargetMapping(sourceType, targetType, elementMapping, insertMethodName, ensureCapacityBuilder)
        )
    {
        _objectFactory = objectFactory;
        _typeToInstantiate = targetType;
        _countPropertyName = null;
    }

    /// <summary>
    /// warning: if <paramref name="countPropertyName"/> is not null then <paramref name="typeToInstantiate"/> must have a single integer constructor
    /// </summary>
    public ForEachAddEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INewInstanceMapping elementMapping,
        string insertMethodName,
        ITypeSymbol? typeToInstantiate,
        string? countPropertyName
    )
        : base(new ForEachAddEnumerableExistingTargetMapping(sourceType, targetType, elementMapping, insertMethodName, null))
    {
        _objectFactory = null;
        _typeToInstantiate = typeToInstantiate ?? targetType;
        _countPropertyName = countPropertyName;
    }

    protected override ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx)
    {
        if (_objectFactory != null)
            return _objectFactory.CreateType(SourceType, _typeToInstantiate, ctx.Source);

        if (_countPropertyName != null)
            return CreateInstance(_typeToInstantiate, MemberAccess(ctx.Source, _countPropertyName));

        return CreateInstance(_typeToInstantiate);
    }
}
