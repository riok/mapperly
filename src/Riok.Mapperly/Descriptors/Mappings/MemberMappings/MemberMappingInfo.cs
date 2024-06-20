using System.Diagnostics;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

[DebuggerDisplay("{DebuggerDisplay}")]
public record MemberMappingInfo(MemberPath SourceMember, NonEmptyMemberPath TargetMember, MemberMappingConfiguration? Configuration = null)
{
    private string DebuggerDisplay => $"{SourceMember.FullName} => {TargetMember.FullName}";

    public TypeMappingKey ToTypeMappingKey()
    {
        if (SourceMember == null)
            throw new InvalidOperationException($"{SourceMember} and {TargetMember} need to be set to create a {nameof(TypeMappingKey)}");

        return new TypeMappingKey(SourceMember.MemberType, TargetMember.MemberType, Configuration?.ToTypeMappingConfiguration());
    }

    public string DescribeSource() => SourceMember.ToDisplayString(includeMemberType: false);
}
