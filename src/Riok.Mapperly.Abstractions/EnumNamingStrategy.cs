using System.ComponentModel;
using System.Runtime.Serialization;

namespace Riok.Mapperly.Abstractions;

/// <summary>
/// Defines the strategy to use when mapping an enum from/to string.
/// </summary>
public enum EnumNamingStrategy
{
    /// <summary>
    /// Matches enum values using their name.
    /// </summary>
    MemberName,

    /// <summary>
    /// Matches enum values using camelCase.
    /// </summary>
    CamelCase,

    /// <summary>
    /// Matches enum values using PascalCase.
    /// </summary>
    PascalCase,

    /// <summary>
    /// Matches enum values using snake_case.
    /// </summary>
    SnakeCase,

    /// <summary>
    /// Matches enum values using UPPER_SNAKE_CASE.
    /// </summary>
    UpperSnakeCase,

    /// <summary>
    /// Matches enum values using kebab-case.
    /// </summary>
    KebabCase,

    /// <summary>
    /// Matches enum values using UPPER-KEBAB-CASE.
    /// </summary>
    UpperKebabCase,

    /// <summary>
    /// Matches enum values using <see cref="DescriptionAttribute.Description"/>
    /// or <see cref="MemberName"/> if the attribute is not present on the enum member.
    /// </summary>
    ComponentModelDescriptionAttribute,

    /// <summary>
    /// Matches enum values using <see cref="EnumMemberAttribute.Value"/>
    /// or <see cref="MemberName"/> if the attribute is not present on the enum member.
    /// </summary>
    SerializationEnumMemberAttribute,
}
