using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A mapped source member without any null handling.
/// (e.g. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class MappedMemberSourceValue(
    INewInstanceMapping delegateMapping,
    GetterMemberPath sourceGetter,
    bool nullConditionalAccess,
    bool addValuePropertyOnNullable
) : ISourceValue
{
    public bool RequiresSourceNullCheck => !nullConditionalAccess && sourceGetter.MemberPath.IsAnyNullable();

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(sourceGetter.BuildAccess(ctx.Source, addValuePropertyOnNullable, nullConditionalAccess));
        return delegateMapping.Build(ctx);
    }
}
