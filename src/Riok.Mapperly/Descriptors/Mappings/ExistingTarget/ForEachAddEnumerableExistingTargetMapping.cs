using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableExistingTargetMapping : ExistingTargetMapping
{
    private const string LoopItemVariableName = "item";

    private readonly ITypeMapping _elementMapping;
    private readonly string _insertMethodName;
    private readonly EnsureCapacity? _ensureCapacityBuilder;

    public ForEachAddEnumerableExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping,
        string insertMethodName,
        EnsureCapacity? ensureCapacityBuilder
    )
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
        _insertMethodName = insertMethodName;
        _ensureCapacityBuilder = ensureCapacityBuilder;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var (loopItemCtx, loopItemVariableName) = ctx.WithNewSource(LoopItemVariableName);
        var convertedSourceItemExpression = _elementMapping.Build(loopItemCtx);
        var addMethod = MemberAccess(target, _insertMethodName);

        if (_ensureCapacityBuilder != null)
        {
            yield return _ensureCapacityBuilder.Build(ctx, target);
        }

        yield return ForEachStatement(
            VarIdentifier,
            Identifier(loopItemVariableName),
            ctx.Source,
            Block(ExpressionStatement(Invocation(addMethod, convertedSourceItemExpression)))
        );
    }
}
