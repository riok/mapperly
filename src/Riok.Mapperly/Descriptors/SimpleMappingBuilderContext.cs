using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext
{
    private readonly MapperDescriptor _descriptor;
    private readonly SourceProductionContext _context;
    private readonly MapperConfiguration _configuration;

    public SimpleMappingBuilderContext(
        Compilation compilation,
        MapperConfiguration configuration,
        WellKnownTypes types,
        SymbolAccessor symbolAccessor,
        MapperDescriptor descriptor,
        SourceProductionContext context,
        MappingBuilder mappingBuilder,
        ExistingTargetMappingBuilder existingTargetMappingBuilder
    )
    {
        Compilation = compilation;
        Types = types;
        SymbolAccessor = symbolAccessor;
        _configuration = configuration;
        _descriptor = descriptor;
        _context = context;
        MappingBuilder = mappingBuilder;
        ExistingTargetMappingBuilder = existingTargetMappingBuilder;
    }

    protected SimpleMappingBuilderContext(SimpleMappingBuilderContext ctx)
        : this(
            ctx.Compilation,
            ctx._configuration,
            ctx.Types,
            ctx.SymbolAccessor,
            ctx._descriptor,
            ctx._context,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder
        ) { }

    public Compilation Compilation { get; }

    public MapperAttribute MapperConfiguration => _configuration.Mapper;

    public WellKnownTypes Types { get; }
    public SymbolAccessor SymbolAccessor { get; }

    protected MappingBuilder MappingBuilder { get; }

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; }

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        MapperConfiguration.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs) =>
        ReportDiagnostic(descriptor, location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(), messageArgs);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, SyntaxNode? location, params object[] messageArgs) =>
        ReportDiagnostic(descriptor, location?.GetLocation(), messageArgs);

    protected MappingConfiguration ReadConfiguration(IMethodSymbol? userSymbol) => _configuration.ForMethod(userSymbol);

    private void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs) =>
        _context.ReportDiagnostic(Diagnostic.Create(descriptor, location ?? _descriptor.Syntax.GetLocation(), messageArgs));
}
