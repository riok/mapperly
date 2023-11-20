using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a simple <see cref="IMemberMapping"/> implementation without any null handling.
/// (eg. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class MemberMapping(
    INewInstanceMapping delegateMapping,
    GetterMemberPath sourcePath,
    bool nullConditionalAccess,
    bool addValuePropertyOnNullable
) : IMemberMapping
{
    public GetterMemberPath SourcePath { get; } = sourcePath;

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(SourcePath.BuildAccess(ctx.Source, addValuePropertyOnNullable, nullConditionalAccess));
        return delegateMapping.Build(ctx);
    }
}
