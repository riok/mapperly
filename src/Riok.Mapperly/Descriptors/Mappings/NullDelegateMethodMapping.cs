using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Null aware delegate mapping for <see cref="MethodMapping"/>s.
/// Abstracts handling null values of the delegated mapping.
/// </summary>
public class NullDelegateMethodMapping(
    ITypeSymbol nullableSourceType,
    ITypeSymbol nullableTargetType,
    MethodMapping delegateMapping,
    NullFallbackValue nullFallbackValue
) : MethodMapping(nullableSourceType, nullableTargetType)
{
    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var body = delegateMapping.BuildBody(ctx);
        return AddPreNullHandling(ctx, body);
    }

    private IEnumerable<StatementSyntax> AddPreNullHandling(TypeMappingBuildContext ctx, IEnumerable<StatementSyntax> body)
    {
        if (!SourceType.IsNullable() || delegateMapping.SourceType.IsNullable())
            return body;

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // if (source == null)
        //   return <null-substitute>;
        var fallbackExpression = NullSubstitute(TargetType, ctx.Source, nullFallbackValue);
        var ifExpression = ctx.SyntaxFactory.IfNullReturnOrThrow(ctx.Source, fallbackExpression);
        return body.Prepend(ifExpression);
    }
}
