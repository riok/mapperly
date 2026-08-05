using System.Diagnostics;
using Riok.Mapperly.Configuration.MethodReferences;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration;

[DebuggerDisplay("{Source} => {Target}")]
public record MemberMappingConfiguration(IMemberPathConfiguration Source, IMemberPathConfiguration Target) : HasSyntaxReference
{
    /// <summary>
    /// Used to adapt from <see cref="Abstractions.MapPropertyFromSourceAttribute"/>
    /// </summary>
    public MemberMappingConfiguration(IMemberPathConfiguration Target)
        : this(Source: StringMemberPath.Empty, Target) { }

    public string? StringFormat { get; set; }

    public string? FormatProvider { get; set; }

    public IMethodReferenceConfiguration? Use { get; set; }

    public bool SuppressNullMismatchDiagnostic { get; set; }

    /// <summary>
    /// Whether the <see cref="Source"/> refers to an additional mapping method parameter instead of a source type member.
    /// </summary>
    public bool SourceIsParameter { get; set; }

    public bool IsValid => Use == null || FormatProvider == null && StringFormat == null;

    public TypeMappingConfiguration ToTypeMappingConfiguration() =>
        new(StringFormat, FormatProvider, Use?.FullName, SuppressNullMismatchDiagnostic);
}
