using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.ExternalMappings;
using Riok.Mapperly.Descriptors.MappingBodyBuilders;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class DescriptorBuilder
{
    private readonly MapperDescriptor _mapperDescriptor;
    private readonly SymbolAccessor _symbolAccessor;

    private readonly MappingCollection _mappings = new();
    private readonly MethodNameBuilder _methodNameBuilder = new();
    private readonly MappingBodyBuilder _mappingBodyBuilder;
    private readonly SimpleMappingBuilderContext _builderContext;
    private readonly List<Diagnostic> _diagnostics = new();

    private ObjectFactoryCollection _objectFactories = ObjectFactoryCollection.Empty;

    public DescriptorBuilder(
        CompilationContext compilationContext,
        MapperDeclaration mapperDeclaration,
        SymbolAccessor symbolAccessor,
        MapperConfiguration defaultMapperConfiguration
    )
    {
        _mapperDescriptor = new MapperDescriptor(mapperDeclaration, _methodNameBuilder);
        _symbolAccessor = symbolAccessor;
        _mappingBodyBuilder = new MappingBodyBuilder(_mappings);

        var attributeAccessor = new AttributeDataAccessor(symbolAccessor);
        _builderContext = new SimpleMappingBuilderContext(
            compilationContext,
            new MapperConfigurationReader(attributeAccessor, mapperDeclaration.Symbol, defaultMapperConfiguration),
            _symbolAccessor,
            attributeAccessor,
            _mapperDescriptor,
            _diagnostics,
            new MappingBuilder(_mappings),
            new ExistingTargetMappingBuilder(_mappings)
        );
    }

    public (MapperDescriptor descriptor, IReadOnlyCollection<Diagnostic> diagnostics) Build(CancellationToken cancellationToken)
    {
        ReserveMethodNames();
        ExtractObjectFactories();
        ExtractUserMappings();
        ExtractExternalMappings();
        _mappingBodyBuilder.BuildMappingBodies(cancellationToken);
        BuildMappingMethodNames();
        BuildReferenceHandlingParameters();
        AddMappingsToDescriptor();
        return (_mapperDescriptor, _diagnostics);
    }

    private void ExtractObjectFactories()
    {
        _objectFactories = ObjectFactoryBuilder.ExtractObjectFactories(_builderContext, _mapperDescriptor.Symbol);
    }

    private void ExtractUserMappings()
    {
        foreach (var userMapping in UserMethodMappingExtractor.ExtractUserMappings(_builderContext, _mapperDescriptor.Symbol))
        {
            var ctx = new MappingBuilderContext(
                _builderContext,
                _objectFactories,
                userMapping.Method,
                userMapping.SourceType,
                userMapping.TargetType
            );

            _mappings.Add(userMapping);
            _mappings.EnqueueToBuildBody(userMapping, ctx);
        }
    }

    private void ExtractExternalMappings()
    {
        foreach (var externalMapping in ExternalMappingsExtractor.ExtractExternalMappings(_builderContext, _mapperDescriptor.Symbol))
        {
            _mappings.Add(externalMapping);
        }
    }

    private void ReserveMethodNames()
    {
        foreach (var methodSymbol in _symbolAccessor.GetAllMembers(_mapperDescriptor.Symbol))
        {
            _methodNameBuilder.Reserve(methodSymbol.Name);
        }
    }

    private void BuildMappingMethodNames()
    {
        foreach (var methodMapping in _mappings.MethodMappings)
        {
            methodMapping.SetMethodNameIfNeeded(_methodNameBuilder.Build);
        }
    }

    private void BuildReferenceHandlingParameters()
    {
        if (!_builderContext.MapperConfiguration.UseReferenceHandling)
            return;

        foreach (var methodMapping in _mappings.MethodMappings)
        {
            methodMapping.EnableReferenceHandling(_builderContext.Types.Get<IReferenceHandler>());
        }
    }

    private void AddMappingsToDescriptor()
    {
        // add generated mappings to the mapper
        foreach (var mapping in _mappings.MethodMappings)
        {
            _mapperDescriptor.AddTypeMapping(mapping);
        }
    }
}
