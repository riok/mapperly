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
        _countPropertyName = null;
    }

    /// <summary>
    /// warning: if <paramref name="countPropertyName"/> is not null then <paramref name="targetType"/> must have a single integer constructor
    /// </summary>
    public ForEachAddEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        INewInstanceMapping elementMapping,
        string insertMethodName,
        string? countPropertyName
    )
        : base(new ForEachAddEnumerableExistingTargetMapping(sourceType, targetType, elementMapping, insertMethodName, null))
    {
        _objectFactory = null;
        _countPropertyName = countPropertyName;
    }

    protected override ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx)
    {
        if (_objectFactory != null)
            return _objectFactory.CreateType(SourceType, TargetType, ctx.Source);

        if (_countPropertyName != null)
            return ctx.SyntaxFactory.CreateInstance(TargetType, MemberAccess(ctx.Source, _countPropertyName));

        return CreateInstance(TargetType);
    }
}
