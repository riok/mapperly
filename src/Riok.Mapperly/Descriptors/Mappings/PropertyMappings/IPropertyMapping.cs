using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a property mapping which accesses a source property and maps it to a certain type.
/// (eg. <c>MapToC(source.A.B)</c>)
/// </summary>
public interface IPropertyMapping
{
    PropertyPath SourcePath { get; }

    ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
