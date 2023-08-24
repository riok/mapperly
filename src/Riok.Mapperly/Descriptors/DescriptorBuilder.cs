using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.ExternalMappings;
using Riok.Mapperly.Descriptors.MappingBodyBuilders;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using Riok.Mapperly.Templates;

namespace Riok.Mapperly.Descriptors;

public class DescriptorBuilder
{
    private const string UnsafeAccessorName = "System.Runtime.CompilerServices.UnsafeAccessorAttribute";

    private readonly MapperDescriptor _mapperDescriptor;
    private readonly SymbolAccessor _symbolAccessor;
    private readonly WellKnownTypes _types;

    private readonly MappingCollection _mappings = new();
    private readonly MethodNameBuilder _methodNameBuilder = new();
    private readonly MappingBodyBuilder _mappingBodyBuilder;
    private readonly SimpleMappingBuilderContext _builderContext;
    private readonly List<Diagnostic> _diagnostics = new();
    private readonly UnsafeAccessorContext _unsafeAccessorContext;
    private readonly MapperConfigurationReader _configurationReader;

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
        _types = compilationContext.Types;
        _mappingBodyBuilder = new MappingBodyBuilder(_mappings);
        _unsafeAccessorContext = new UnsafeAccessorContext(_methodNameBuilder, symbolAccessor);

        var attributeAccessor = new AttributeDataAccessor(symbolAccessor);
        _configurationReader = new MapperConfigurationReader(attributeAccessor, mapperDeclaration.Symbol, defaultMapperConfiguration);

        _builderContext = new SimpleMappingBuilderContext(
            compilationContext,
            _configurationReader,
            _symbolAccessor,
            attributeAccessor,
            _mapperDescriptor,
            _unsafeAccessorContext,
            _diagnostics,
            new MappingBuilder(_mappings),
            new ExistingTargetMappingBuilder(_mappings)
        );
    }

    public (MapperDescriptor descriptor, IReadOnlyCollection<Diagnostic> diagnostics) Build(CancellationToken cancellationToken)
    {
        ConfigureMemberVisibility();
        ReserveMethodNames();
        ExtractUserMappings();
        ExtractObjectFactories();
        ExtractExternalMappings();
        _mappingBodyBuilder.BuildMappingBodies(cancellationToken);
        BuildMappingMethodNames();
        TemplateResolver.AddRequiredTemplates(_builderContext.MapperConfiguration, _mappings, _mapperDescriptor);
        BuildReferenceHandlingParameters();
        AddMappingsToDescriptor();
        AddAccessorsToDescriptor();
        return (_mapperDescriptor, _diagnostics);
    }

    /// <summary>
    /// If <see cref="MemberVisibility.Accessible"/> is not set and the roslyn version does not have UnsafeAccessors
    /// then emit a diagnostic and update the <see cref="MemberVisibility"/> for <see cref="SymbolAccessor"/>.
    /// </summary>
    private void ConfigureMemberVisibility()
    {
        var includedMembers = _configurationReader.Mapper.IncludedMembers;

        if (_types.TryGet(UnsafeAccessorName) != null)
        {
            _symbolAccessor.SetMemberVisibility(includedMembers);
            return;
        }

        if (includedMembers.HasFlag(MemberVisibility.Accessible))
            return;

        _diagnostics.Add(
            Diagnostic.Create(Diagnostics.DiagnosticDescriptors.UnsafeAccessorNotAvailable, _mapperDescriptor.Syntax.GetLocation())
        );
        _symbolAccessor.SetMemberVisibility(includedMembers | MemberVisibility.Accessible);
    }

    private void ReserveMethodNames()
    {
        foreach (var methodSymbol in _symbolAccessor.GetAllMembers(_mapperDescriptor.Symbol))
        {
            _methodNameBuilder.Reserve(methodSymbol.Name);
        }
    }

    private void ExtractObjectFactories()
    {
        _objectFactories = ObjectFactoryBuilder.ExtractObjectFactories(_builderContext, _mapperDescriptor.Symbol);
    }

    private void ExtractUserMappings()
    {
        _mapperDescriptor.Static = _mapperDescriptor.Symbol.IsStatic;
        IMethodSymbol? firstNonStaticUserMapping = null;

        foreach (var userMapping in UserMethodMappingExtractor.ExtractUserMappings(_builderContext, _mapperDescriptor.Symbol))
        {
            var ctx = new MappingBuilderContext(
                _builderContext,
                _objectFactories,
                userMapping.Method,
                userMapping.SourceType,
                userMapping.TargetType
            );

            // if a user defined mapping method is static, all of them need to be static to avoid confusion for mapping method resolution
            // however, user implemented mapping methods are allowed to be static in a non-static context.
            // Therefore we are only interested in partial method definitions here.
            if (userMapping.Method is { IsStatic: true, IsPartialDefinition: true })
            {
                _mapperDescriptor.Static = true;
            }
            else if (firstNonStaticUserMapping == null && !userMapping.Method.IsStatic)
            {
                firstNonStaticUserMapping = userMapping.Method;
            }

            _mappings.Add(userMapping);
            _mappings.EnqueueToBuildBody(userMapping, ctx);
        }

        if (_mapperDescriptor.Static && firstNonStaticUserMapping is not null)
        {
            _diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.MixingStaticPartialWithInstanceMethod,
                    firstNonStaticUserMapping.Locations.FirstOrDefault(),
                    _mapperDescriptor.Symbol.ToDisplayString()
                )
            );
        }
    }

    private void ExtractExternalMappings()
    {
        foreach (var externalMapping in ExternalMappingsExtractor.ExtractExternalMappings(_builderContext, _mapperDescriptor.Symbol))
        {
            _mappings.Add(externalMapping);
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

    private void AddAccessorsToDescriptor()
    {
        // add generated accessors to the mapper
        _mapperDescriptor.AddUnsafeAccessors(_unsafeAccessorContext.UnsafeAccessors);
    }
}
