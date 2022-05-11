using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext
{
    private readonly DescriptorBuilder _builder;

    public SimpleMappingBuilderContext(DescriptorBuilder builder)
    {
        _builder = builder;
    }

    public Compilation Compilation => _builder.Compilation;

    public MapperAttribute MapperConfiguration => _builder.MapperConfiguration;

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs)
        => ReportDiagnostic(descriptor, location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(), messageArgs);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, SyntaxNode? location, params object[] messageArgs)
        => ReportDiagnostic(descriptor, location?.GetLocation(), messageArgs);

    private void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        => _builder.ReportDiagnostic(descriptor, location, messageArgs);
}
