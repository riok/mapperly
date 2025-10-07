using System.Diagnostics;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Configuration.PropertyReferences;

namespace Riok.Mapperly.Configuration;

[DebuggerDisplay("{Target}: {DescribeValue()}")]
public record MemberValueMappingConfiguration(IMemberPathConfiguration Target, AttributeValue? Value) : HasSyntaxReference
{
    /// <summary>
    /// Constructor used by <see cref="AttributeDataAccessor"/>.
    /// </summary>
    public MemberValueMappingConfiguration(IMemberPathConfiguration target)
        : this(target, null) { }

    public IMethodReferenceConfiguration? Use { get; set; }

    public bool IsValid => Use != null ^ Value != null;

    public string DescribeValue()
    {
        if (Use != null)
            return Use + "()";

        return Value?.Expression.ToFullString() ?? string.Empty;
    }
}
