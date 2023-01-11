using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a simple <see cref="IPropertyMapping"/> implementation without any null handling.
/// (eg. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class PropertyMapping : IPropertyMapping
{
    private readonly ITypeMapping _delegateMapping;
    private readonly bool _nullConditionalAccess;
    private readonly bool _addValuePropertyOnNullable;

    public PropertyMapping(ITypeMapping delegateMapping, PropertyPath sourcePath, bool nullConditionalAccess, bool addValuePropertyOnNullable)
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        _nullConditionalAccess = nullConditionalAccess;
        _addValuePropertyOnNullable = addValuePropertyOnNullable;
    }

    public PropertyPath SourcePath { get; }

    public ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        ctx = ctx.WithSource(SourcePath.BuildAccess(ctx.Source, _addValuePropertyOnNullable, _nullConditionalAccess));
        return _delegateMapping.Build(ctx);
    }
}
