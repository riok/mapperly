using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A mapped source member without any null handling.
/// (e.g. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class MappedMemberSourceValue(
    INewInstanceMapping delegateMapping,
    MemberPathGetter sourceMember,
    bool nullConditionalAccess,
    bool addValuePropertyOnNullable
) : ISourceValue
{
    public bool RequiresSourceNullCheck => !nullConditionalAccess && sourceMember.MemberPath.IsAnyNullable();

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(
            sourceMember.BuildAccess(
                ctx.Source,
                addValuePropertyOnNullable: addValuePropertyOnNullable,
                nullConditional: nullConditionalAccess
            )
        );
        return delegateMapping.Build(ctx);
    }
}
