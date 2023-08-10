using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

/// <summary>
/// Represents a container of several <see cref="IMemberAssignmentMapping"/>.
/// </summary>
public interface IMemberAssignmentMappingContainer
{
    IMemberAssignmentMapping? TryGetMemberMapping(MemberPath sourceMemberPath);

    bool HasMemberMapping(IMemberAssignmentMapping mapping);

    void AddMemberMapping(IMemberAssignmentMapping mapping);

    bool HasMemberMappingContainer(IMemberAssignmentMappingContainer container);

    void AddMemberMappingContainer(IMemberAssignmentMappingContainer container);

    IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax targetAccess);
}
