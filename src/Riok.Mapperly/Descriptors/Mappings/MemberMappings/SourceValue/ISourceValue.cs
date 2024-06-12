using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings.SourceValue;

/// <summary>
/// A source value is the right part of an <see cref="MemberAssignmentMapping"/>.
/// It can be a constant value or a mapped member of the mapping source object.
/// </summary>
public interface ISourceValue
{
    ExpressionSyntax Build(TypeMappingBuildContext ctx);
}
