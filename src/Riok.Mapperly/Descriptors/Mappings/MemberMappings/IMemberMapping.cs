using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a member mapping which accesses a source member and maps it to a certain type.
/// (eg. <c>MapToC(source.A.B)</c>)
/// </summary>
public interface IMemberMapping
{
    MemberPathGetterBuilder SourceGetter { get; }

    ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
