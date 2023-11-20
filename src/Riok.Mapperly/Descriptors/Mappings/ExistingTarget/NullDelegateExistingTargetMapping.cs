using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Wraps a <see cref="ExistingTargetMapping"/> with <c>null</c> handling.
/// Does not call the <see cref="ExistingTargetMapping"/> when
/// the target or source is <c>null</c>.
/// </summary>
public class NullDelegateExistingTargetMapping(
    ITypeSymbol nullableSourceType,
    ITypeSymbol nullableTargetType,
    IExistingTargetMapping delegateMapping
) : ExistingTargetMapping(nullableSourceType, nullableTargetType)
{
    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // if the source or target type is nullable, add a null guard.
        if (!SourceType.IsNullable() && !TargetType.IsNullable())
            return delegateMapping.Build(ctx, target);

        var body = delegateMapping.Build(ctx.AddIndentation(), target).ToArray();

        // if body is empty don't generate an if statement
        if (body.Length == 0)
        {
            return Enumerable.Empty<StatementSyntax>();
        }

        // if (source != null && target != null) { body }
        var condition = IfNoneNull((SourceType, ctx.Source), (TargetType, target));
        var ifStatement = ctx.SyntaxFactory.If(condition, body);
        return new[] { ifStatement };
    }
}
