using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a simple <see cref="IMemberMapping"/> implementation without any null handling.
/// (eg. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class MemberMapping : IMemberMapping
{
    private readonly INewInstanceMapping _delegateMapping;
    private readonly bool _addValuePropertyOnNullable;

    public MemberMapping(
        INewInstanceMapping delegateMapping,
        GetterMemberPath sourcePath,
        bool nullConditionalAccess,
        bool addValuePropertyOnNullable
    )
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        NullConditionalAccess = nullConditionalAccess;
        _addValuePropertyOnNullable = addValuePropertyOnNullable;
    }

    public GetterMemberPath SourcePath { get; }
    public bool NullConditionalAccess { get; }

    public ITypeSymbol SourceType => _delegateMapping.SourceType;

    public ITypeSymbol TargetType => _delegateMapping.TargetType;

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(SourcePath.BuildAccess(ctx.Source, _addValuePropertyOnNullable, NullConditionalAccess));
        return _delegateMapping.Build(ctx);
    }
}
