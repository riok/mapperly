using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Null aware delegate mapping for <see cref="MethodMapping"/>s.
/// Abstracts handling null values of the delegated mapping.
/// </summary>
public class NullDelegateMethodMapping : MethodMapping
{
    private readonly MethodMapping _delegateMapping;
    private readonly NullFallbackValue _nullFallbackValue;

    public NullDelegateMethodMapping(
        ITypeSymbol nullableSourceType,
        ITypeSymbol nullableTargetType,
        MethodMapping delegateMapping,
        NullFallbackValue nullFallbackValue
    )
        : base(nullableSourceType, nullableTargetType)
    {
        _delegateMapping = delegateMapping;
        _nullFallbackValue = nullFallbackValue;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var body = _delegateMapping.BuildBody(ctx);
        return AddPreNullHandling(ctx.Source, body);
    }

    private IEnumerable<StatementSyntax> AddPreNullHandling(ExpressionSyntax source, IEnumerable<StatementSyntax> body)
    {
        if (!SourceType.IsNullable() || _delegateMapping.SourceType.IsNullable())
            return body;

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // if (source == null)
        //   return <null-substitute>;
        return body.Prepend(IfNullReturnOrThrow(source, NullSubstitute(TargetType.NonNullable(), source, _nullFallbackValue)));
    }
}
