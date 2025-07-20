using System.Diagnostics;

namespace Riok.Mapperly.Configuration;

[DebuggerDisplay("{Target}: {DescribeValue()}")]
public record MemberValueMappingConfiguration(IMemberPathConfiguration Target, AttributeValue? Value) : HasSyntaxReference
{
    /// <summary>
    /// Constructor used by <see cref="AttributeDataAccessor"/>.
    /// </summary>
    public MemberValueMappingConfiguration(IMemberPathConfiguration target)
        : this(target, null) { }

    public MethodReferenceConfiguration? Use { get; set; }

    public bool IsValid => Use != null ^ Value != null;

    public string DescribeValue()
    {
        if (Use != null)
            return Use.ToString();

        return Value?.Expression.ToFullString() ?? string.Empty;
    }
}
