using System.Diagnostics;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.MemberMappings;

[DebuggerDisplay("{DebuggerDisplay}")]
public record MemberMappingInfo(
    MemberPath? SourceMember,
    NonEmptyMemberPath TargetMember,
    MemberValueMappingConfiguration? ValueConfiguration = null,
    MemberMappingConfiguration? Configuration = null
)
{
    public MemberMappingInfo(MemberPath? sourceMember, NonEmptyMemberPath targetMember, MemberMappingConfiguration? configuration)
        : this(sourceMember, targetMember, null, configuration) { }

    public MemberMappingInfo(NonEmptyMemberPath targetMember, MemberValueMappingConfiguration configuration)
        : this(null, targetMember, configuration) { }

    private string DebuggerDisplay => $"{SourceMember?.FullName ?? ValueConfiguration?.DescribeValue()} => {TargetMember.FullName}";

    public TypeMappingKey ToTypeMappingKey()
    {
        if (SourceMember == null)
            throw new InvalidOperationException($"{SourceMember} and {TargetMember} need to be set to create a {nameof(TypeMappingKey)}");

        return new TypeMappingKey(SourceMember.MemberType, TargetMember.MemberType, Configuration?.ToTypeMappingConfiguration());
    }

    public string DescribeSource()
    {
        return SourceMember?.ToDisplayString(includeMemberType: false) ?? ValueConfiguration?.DescribeValue() ?? string.Empty;
    }
}
