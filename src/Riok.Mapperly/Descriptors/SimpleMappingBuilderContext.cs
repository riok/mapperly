using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext
{
    public SimpleMappingBuilderContext(DescriptorBuilder builder)
    {
        Builder = builder;
    }

    protected DescriptorBuilder Builder { get; }

    public Compilation Compilation => Builder.Compilation;

    public MapperAttribute MapperConfiguration => Builder.MapperConfiguration;

    public bool IsConversionEnabled(MappingConversionType conversionType)
        => MapperConfiguration.EnabledConversions.HasFlag(conversionType);

    public WellKnownTypes Types => Builder.WellKnownTypes;

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs)
        => ReportDiagnostic(descriptor, location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(), messageArgs);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, SyntaxNode? location, params object[] messageArgs)
        => ReportDiagnostic(descriptor, location?.GetLocation(), messageArgs);

    private void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        => Builder.ReportDiagnostic(descriptor, location, messageArgs);
}
