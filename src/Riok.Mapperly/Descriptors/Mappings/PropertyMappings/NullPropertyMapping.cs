using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a null safe <see cref="IPropertyMapping"/>.
/// (eg. <c>source?.A?.B ?? null-substitute</c> or <c>source?.A?.B == null ? null-substitute : MapToD(source.A.B)</c>)
/// </summary>
[DebuggerDisplay("NullPropertyMapping({SourcePath}: {_delegateMapping})")]
public class NullPropertyMapping : IPropertyMapping
{
    private readonly ITypeMapping _delegateMapping;
    private readonly NullFallbackValue _nullFallback;

    public NullPropertyMapping(ITypeMapping delegateMapping, PropertyPath sourcePath, NullFallbackValue nullFallback)
    {
        SourcePath = sourcePath;
        _delegateMapping = delegateMapping;
        _nullFallback = nullFallback;
    }

    public PropertyPath SourcePath { get; }

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping.SourceType.IsNullable() || !SourcePath.IsAnyNullable())
        {
            ctx = ctx.WithSource(SourcePath.BuildAccess(ctx.Source, nullConditional: true));
            return _delegateMapping.Build(ctx);
        }

        // source is nullable and the mapping method cannot handle nulls,
        // call mapping only if source is not null.
        // source.A?.B == null ? <null-substitute> : Map(source.A.B)
        // or for nullable value types:
        // source.A?.B == null ? <null-substitute> : Map(source.A.B.Value)
        // use simplified coalesce expression for direct assignments:
        // source.A?.B ?? <null-substitute>
        if (_delegateMapping.IsSynthetic)
        {
            var nullConditionalSourceAccess = SourcePath.BuildAccess(ctx.Source, nullConditional: true);
            return Coalesce(
                _delegateMapping.Build(ctx.WithSource(nullConditionalSourceAccess)),
                NullSubstitute(_delegateMapping.TargetType, nullConditionalSourceAccess, _nullFallback));
        }

        var nullCheckPath = SourcePath.BuildAccess(ctx.Source, nullConditional: true, skipTrailingNonNullable: true);
        var sourcePropertyAccess = SourcePath.BuildAccess(ctx.Source, true);
        return ConditionalExpression(
            IsNull(nullCheckPath),
            NullSubstitute(_delegateMapping.TargetType, sourcePropertyAccess, _nullFallback),
            _delegateMapping.Build(ctx.WithSource(sourcePropertyAccess)));
    }

    protected bool Equals(NullPropertyMapping other)
        => _delegateMapping.Equals(other._delegateMapping) && _nullFallback == other._nullFallback && SourcePath.Equals(other.SourcePath);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((NullPropertyMapping)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = _delegateMapping.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)_nullFallback;
            hashCode = (hashCode * 397) ^ SourcePath.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(NullPropertyMapping? left, NullPropertyMapping? right)
        => Equals(left, right);

    public static bool operator !=(NullPropertyMapping? left, NullPropertyMapping? right)
        => !Equals(left, right);
}
