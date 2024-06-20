using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A mapped source member with additional null handling.
/// (e.g. <c>source?.A?.B ?? null-substitute</c> or <c>source?.A?.B != null ? MapToD(source.A.B) : null-substitute</c>)
/// </summary>
[DebuggerDisplay("NullMappedMemberSourceValue({_sourceGetter}: {_delegateMapping})")]
public class NullMappedMemberSourceValue(
    INewInstanceMapping delegateMapping,
    GetterMemberPath sourceGetter,
    ITypeSymbol targetType,
    NullFallbackValue nullFallback,
    bool useNullConditionalAccess
) : ISourceValue
{
    private readonly INewInstanceMapping _delegateMapping = delegateMapping;
    private readonly NullFallbackValue _nullFallback = nullFallback;
    private readonly GetterMemberPath _sourceGetter = sourceGetter;

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // the source type of the delegate mapping is nullable or the source path is not nullable
        // build mapping with null conditional access
        if (_delegateMapping.SourceType.IsNullable() || !_sourceGetter.MemberPath.IsAnyNullable())
        {
            ctx = ctx.WithSource(_sourceGetter.BuildAccess(ctx.Source, nullConditional: true));
            return _delegateMapping.Build(ctx);
        }

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // source.A?.B == null ? <null-substitute> : Map(source.A.B)
        // or for nullable value types:
        // source.A?.B == null ? <null-substitute> : Map(source.A.B.Value)
        // use simplified coalesce expression for synthetic mappings:
        // source.A?.B ?? <null-substitute>
        if (_delegateMapping.IsSynthetic && (useNullConditionalAccess || !_sourceGetter.MemberPath.IsAnyObjectPathNullable()))
        {
            var nullConditionalSourceAccess = _sourceGetter.BuildAccess(ctx.Source, nullConditional: true);
            var nameofSourceAccess = _sourceGetter.BuildAccess(ctx.Source, nullConditional: false);
            var mapping = _delegateMapping.Build(ctx.WithSource(nullConditionalSourceAccess));
            return _nullFallback == NullFallbackValue.Default && targetType.IsNullable()
                ? mapping
                : Coalesce(mapping, NullSubstitute(targetType, nameofSourceAccess, _nullFallback));
        }

        var notNullCondition = useNullConditionalAccess
            ? IsNotNull(_sourceGetter.BuildAccess(ctx.Source, nullConditional: true, skipTrailingNonNullable: true))
            : _sourceGetter.MemberPath.BuildNonNullConditionWithoutConditionalAccess(ctx.Source)!;
        var sourceMemberAccess = _sourceGetter.BuildAccess(ctx.Source, true);
        ctx = ctx.WithSource(sourceMemberAccess);
        return Conditional(notNullCondition, _delegateMapping.Build(ctx), NullSubstitute(targetType, sourceMemberAccess, _nullFallback));
    }

    protected bool Equals(NullMappedMemberSourceValue other)
    {
        return _delegateMapping.Equals(other._delegateMapping)
            && _nullFallback == other._nullFallback
            && _sourceGetter.Equals(other._sourceGetter);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((NullMappedMemberSourceValue)obj);
    }

    public override int GetHashCode() => HashCode.Combine(_delegateMapping, _nullFallback, _sourceGetter);

    public static bool operator ==(NullMappedMemberSourceValue? left, NullMappedMemberSourceValue? right) => Equals(left, right);

    public static bool operator !=(NullMappedMemberSourceValue? left, NullMappedMemberSourceValue? right) => !Equals(left, right);
}
