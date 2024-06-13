using System.Diagnostics;

namespace Riok.Mapperly.Configuration;

[DebuggerDisplay("{Target}: {DescribeValue()}")]
public record MemberValueMappingConfiguration(StringMemberPath Target, AttributeValue? Value) : HasSyntaxReference
{
    /// <summary>
    /// Constructor used by <see cref="AttributeDataAccessor"/>.
    /// </summary>
    public MemberValueMappingConfiguration(StringMemberPath target)
        : this(target, null) { }

    public string? Use { get; set; }

    public bool IsValid => Use != null ^ Value != null;

    public string DescribeValue()
    {
        if (Use != null)
            return Use + "()";

        return Value?.Expression.ToFullString() ?? string.Empty;
    }
}
