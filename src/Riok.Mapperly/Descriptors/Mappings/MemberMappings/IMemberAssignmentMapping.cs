using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a member assignment mapping or a container of member assignment mappings.
/// </summary>
public interface IMemberAssignmentMapping
{
    MemberPath SourcePath { get; }

    MemberPath TargetPath { get; }

    IEnumerable<StatementSyntax> Build(
        TypeMappingBuildContext ctx,
        ExpressionSyntax targetAccess);
}
