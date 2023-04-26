using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Wraps a <see cref="ExistingTargetMapping"/> with <c>null</c> handling.
/// Does not call the <see cref="ExistingTargetMapping"/> when
/// the target or source is <c>null</c>.
/// </summary>
public class NullDelegateExistingTargetMapping : ExistingTargetMapping
{
    private readonly IExistingTargetMapping _delegateMapping;

    public NullDelegateExistingTargetMapping(
        ITypeSymbol nullableSourceType,
        ITypeSymbol nullableTargetType,
        IExistingTargetMapping delegateMapping
    )
        : base(nullableSourceType, nullableTargetType)
    {
        _delegateMapping = delegateMapping;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var body = _delegateMapping.Build(ctx, target);

        // if the source or target type is nullable, add a null guard.
        if (!SourceType.IsNullable() && !TargetType.IsNullable())
            return body;

        // if (source != null && target != null) { body }
        return new[] { IfStatement(IfNoneNull((SourceType, ctx.Source), (TargetType, target)), Block(body)), };
        ;
    }
}
