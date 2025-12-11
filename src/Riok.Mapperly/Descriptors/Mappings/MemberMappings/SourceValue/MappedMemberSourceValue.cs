using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A mapped source member without any null handling.
/// (e.g. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
[DebuggerDisplay("MappedMemberSourceValue({_sourceMember}: {_delegateMapping})")]
public class MappedMemberSourceValue(
    INewInstanceMapping delegateMapping,
    MemberPathGetter sourceMember,
    bool nullConditionalAccess,
    bool addValuePropertyOnNullable
) : ISourceValue, IHasUsedParameters
{
    private readonly MemberPathGetter _sourceMember = sourceMember;
    private readonly INewInstanceMapping _delegateMapping = delegateMapping;

    public bool RequiresSourceNullCheck => !nullConditionalAccess && _sourceMember.MemberPath.IsAnyNullable();

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(
            _sourceMember.BuildAccess(
                ctx.Source,
                addValuePropertyOnNullable: addValuePropertyOnNullable,
                nullConditional: nullConditionalAccess
            )
        );
        return _delegateMapping.Build(ctx);
    }

    public IEnumerable<string> ExtractUsedParameters() => UsedParameterHelpers.ExtractUsedParameters(_delegateMapping);
}
