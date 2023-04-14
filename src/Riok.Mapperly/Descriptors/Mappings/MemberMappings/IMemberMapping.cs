using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a member mapping which accesses a source member and maps it to a certain type.
/// (eg. <c>MapToC(source.A.B)</c>)
/// </summary>
public interface IMemberMapping
{
    MemberPath SourcePath { get; }

    ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
