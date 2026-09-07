using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Configuration.PropertyReferences;

namespace Riok.Mapperly.Configuration;

/// <summary>
/// A member mapping configuration where the source is an additional mapping method parameter.
/// </summary>
public record ParameterMappingConfiguration(IMemberPathConfiguration Target, string Parameter) : HasSyntaxReference
{
    public string? StringFormat { get; set; }

    public string? FormatProvider { get; set; }

    public IMethodReferenceConfiguration? Use { get; set; }

    public bool SuppressNullMismatchDiagnostic { get; set; }

    public MemberMappingConfiguration ToMemberMappingConfiguration() =>
        new(new StringMemberPath([Parameter]), Target)
        {
            StringFormat = StringFormat,
            FormatProvider = FormatProvider,
            Use = Use,
            SuppressNullMismatchDiagnostic = SuppressNullMismatchDiagnostic,
            SyntaxReference = SyntaxReference,
            SourceIsParameter = true,
        };
}
