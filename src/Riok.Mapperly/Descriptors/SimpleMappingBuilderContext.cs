using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.MappingBuilders;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext
{
    private readonly MapperDescriptor _descriptor;
    private readonly SourceProductionContext _context;

    public SimpleMappingBuilderContext(
        Compilation compilation,
        Configuration configuration,
        WellKnownTypes types,
        MapperDescriptor descriptor,
        SourceProductionContext context,
        MappingBuilder mappingBuilder,
        ExistingTargetMappingBuilder existingTargetMappingBuilder)
    {
        Compilation = compilation;
        Types = types;
        Configuration = configuration;
        _descriptor = descriptor;
        _context = context;
        MappingBuilder = mappingBuilder;
        ExistingTargetMappingBuilder = existingTargetMappingBuilder;
    }


    protected SimpleMappingBuilderContext(SimpleMappingBuilderContext ctx)
        : this(
            ctx.Compilation,
            ctx.Configuration,
            ctx.Types,
            ctx._descriptor,
            ctx._context,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder)
    {
    }
    public Compilation Compilation { get; }

    public MapperAttribute MapperConfiguration => Configuration.Mapper;

    public WellKnownTypes Types { get; }

    protected Configuration Configuration { get; }

    protected MappingBuilder MappingBuilder { get; }

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; }

    public virtual bool IsConversionEnabled(MappingConversionType conversionType)
        => MapperConfiguration.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs)
        => ReportDiagnostic(descriptor, location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(), messageArgs);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, SyntaxNode? location, params object[] messageArgs)
        => ReportDiagnostic(descriptor, location?.GetLocation(), messageArgs);

    private void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        => _context.ReportDiagnostic(Diagnostic.Create(descriptor, location ?? _descriptor.Syntax.GetLocation(), messageArgs));
}
