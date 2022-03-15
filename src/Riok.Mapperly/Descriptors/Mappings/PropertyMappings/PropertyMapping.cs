using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a simple <see cref="IPropertyMapping"/> implementation without any null handling.
/// (eg. <c>MapToD(source.A.B)</c> or <c>MapToD(source?.A?.B)</c>).
/// </summary>
public class PropertyMapping : IPropertyMapping
{
    private readonly TypeMapping _delegateMapping;
    private readonly bool _nullConditionalAccess;

    public PropertyMapping(TypeMapping delegateMapping, PropertyPath sourcePath, bool nullConditionalAccess)
    {
        _delegateMapping = delegateMapping;
        SourcePath = sourcePath;
        _nullConditionalAccess = nullConditionalAccess;
    }

    public PropertyPath SourcePath { get; }

    public ExpressionSyntax Build(ExpressionSyntax source)
        => _delegateMapping.Build(SourcePath.BuildAccess(source, true, _nullConditionalAccess));
}
