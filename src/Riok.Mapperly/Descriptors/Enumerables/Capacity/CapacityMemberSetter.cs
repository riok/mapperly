using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols.Members;

namespace Riok.Mapperly.Descriptors.Enumerables.Capacity;

internal class CapacityMemberSetter(IMappableMember targetCapacityMember, IMemberSetter setter) : ICapacityMemberSetter
{
    public bool SupportsCoalesceAssignment => setter.SupportsCoalesceAssignment;

    public IMappableMember TargetCapacity => targetCapacityMember;

    public ExpressionSyntax BuildAssignment(
        ExpressionSyntax? baseAccess,
        ExpressionSyntax valueToAssign,
        bool coalesceAssignment = false
    ) => setter.BuildAssignment(baseAccess, valueToAssign, coalesceAssignment);

    public static ICapacityMemberSetter Build(MappingBuilderContext ctx, IMappableMember member) =>
        new CapacityMemberSetter(member, member.BuildSetter(ctx.UnsafeAccessorContext));
}
