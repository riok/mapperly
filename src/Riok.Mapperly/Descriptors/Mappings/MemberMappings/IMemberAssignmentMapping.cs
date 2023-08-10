using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a member assignment mapping or a container of member assignment mappings.
/// </summary>
public interface IMemberAssignmentMapping : IMapping
{
    GetterMemberPath SourcePath { get; }

    MemberPath TargetPath { get; }

    IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess);
}
