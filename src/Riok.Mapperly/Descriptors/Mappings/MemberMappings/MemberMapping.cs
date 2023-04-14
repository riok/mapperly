using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a simple <see cref="IMemberMapping"/> implementation without any null handling.
/// (eg. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class MemberMapping : IMemberMapping
{
    private readonly ITypeMapping _delegateMapping;
    private readonly bool _nullConditionalAccess;
    private readonly bool _addValuePropertyOnNullable;

    public MemberMapping(ITypeMapping delegateMapping, MemberPath sourcePath, bool nullConditionalAccess, bool addValuePropertyOnNullable)
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        _nullConditionalAccess = nullConditionalAccess;
        _addValuePropertyOnNullable = addValuePropertyOnNullable;
    }

    public MemberPath SourcePath { get; }

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(SourcePath.BuildAccess(ctx.Source, _addValuePropertyOnNullable, _nullConditionalAccess));
        return _delegateMapping.Build(ctx);
    }
}
