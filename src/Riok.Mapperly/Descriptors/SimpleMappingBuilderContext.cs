using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

/// <summary>
/// A simple mapping context which does not allow to access and build other mappings.
/// </summary>
public class SimpleMappingBuilderContext
{
    private readonly MapperDescriptor _descriptor;
    private readonly List<Diagnostic> _diagnostics;
    private readonly CompilationContext _compilationContext;
    private readonly MapperConfigurationReader _configurationReader;

    public SimpleMappingBuilderContext(
        CompilationContext compilationContext,
        MapperConfigurationReader configurationReader,
        SymbolAccessor symbolAccessor,
        AttributeDataAccessor attributeAccessor,
        MapperDescriptor descriptor,
        UnsafeAccessorContext unsafeAccessorContext,
        List<Diagnostic> diagnostics,
        MappingBuilder mappingBuilder,
        ExistingTargetMappingBuilder existingTargetMappingBuilder
    )
    {
        SymbolAccessor = symbolAccessor;
        _compilationContext = compilationContext;
        _configurationReader = configurationReader;
        _descriptor = descriptor;
        _diagnostics = diagnostics;
        MappingBuilder = mappingBuilder;
        ExistingTargetMappingBuilder = existingTargetMappingBuilder;
        AttributeAccessor = attributeAccessor;
        UnsafeAccessorContext = unsafeAccessorContext;
    }

    protected SimpleMappingBuilderContext(SimpleMappingBuilderContext ctx)
        : this(
            ctx._compilationContext,
            ctx._configurationReader,
            ctx.SymbolAccessor,
            ctx.AttributeAccessor,
            ctx._descriptor,
            ctx.UnsafeAccessorContext,
            ctx._diagnostics,
            ctx.MappingBuilder,
            ctx.ExistingTargetMappingBuilder
        ) { }

    public Compilation Compilation => _compilationContext.Compilation;

    public MapperAttribute MapperConfiguration => _configurationReader.Mapper;

    public WellKnownTypes Types => _compilationContext.Types;

    public bool Static => _descriptor.Static;

    public SymbolAccessor SymbolAccessor { get; }

    public AttributeDataAccessor AttributeAccessor { get; }

    public UnsafeAccessorContext UnsafeAccessorContext { get; }

    protected MappingBuilder MappingBuilder { get; }

    protected ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; }

    public virtual bool IsConversionEnabled(MappingConversionType conversionType) =>
        MapperConfiguration.EnabledConversions.HasFlag(conversionType);

    public void ReportDiagnostic(DiagnosticDescriptor descriptor, ISymbol? location, params object[] messageArgs)
    {
        // cannot use the symbol since it would break the incremental generator
        // due to being different for each compilation.
        for (var i = 0; i < messageArgs.Length; i++)
        {
            if (messageArgs[i] is ISymbol symbol)
            {
                messageArgs[i] = symbol.ToDisplayString();
            }
        }

        var syntaxNode = location?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
        var nodeLocation = syntaxNode?.GetLocation();
        _diagnostics.Add(Diagnostic.Create(descriptor, nodeLocation ?? _descriptor.Syntax.GetLocation(), messageArgs));
    }

    protected MappingConfiguration ReadConfiguration(MappingConfigurationReference configRef) => _configurationReader.BuildFor(configRef);
}
