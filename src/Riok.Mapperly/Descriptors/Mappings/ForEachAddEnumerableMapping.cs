using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableMapping : ExistingTargetMappingMethodWrapper
{
    private readonly ObjectFactory? _objectFactory;

    public ForEachAddEnumerableMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        ObjectFactory? objectFactory,
        string insertMethodName,
        EnsureCapacity? ensureCapacityBuilder)
        : base(new ForEachAddEnumerableExistingTargetMapping(sourceType, targetType, elementMapping, insertMethodName, ensureCapacityBuilder))
    {
        _objectFactory = objectFactory;
    }
    protected override ExpressionSyntax CreateTargetInstance(TypeMappingBuildContext ctx)
    {
        return _objectFactory == null
            ? CreateInstance(TargetType)
            : _objectFactory.CreateType(SourceType, TargetType, ctx.Source);
    }
}
