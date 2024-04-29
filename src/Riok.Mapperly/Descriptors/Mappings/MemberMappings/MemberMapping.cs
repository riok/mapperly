using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a simple <see cref="IMemberMapping"/> implementation without any null handling.
/// (eg. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class MemberMapping(
    INewInstanceMapping delegateMapping,
    MemberPathGetterBuilder sourceGetter,
    bool nullConditionalAccess,
    bool addValuePropertyOnNullable
) : IMemberMapping
{
    public MemberPathGetterBuilder SourceGetter { get; } = sourceGetter;

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(SourceGetter.BuildAccess(ctx.Source, addValuePropertyOnNullable, nullConditionalAccess));
        return delegateMapping.Build(ctx);
    }
}
