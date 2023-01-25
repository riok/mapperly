using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBodyBuilders;
using Riok.Mapperly.Descriptors.MappingBuilders;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class DescriptorBuilder
{
    private readonly SourceProductionContext _context;
    private readonly ITypeSymbol _mapperSymbol;
    private readonly MapperDescriptor _mapperDescriptor;

    // default configurations, used a configuration is needed but no configuration is provided by the user
    // these are the default configurations registered for each configuration attribute.
    // Usually these are derived from the mapper attribute or default values.
    private readonly Dictionary<Type, Attribute> _defaultConfigurations = new();

    private readonly MappingCollection _mappings = new();
    private readonly MethodNameBuilder _methodNameBuilder = new();
    private readonly MappingBodyBuilder _mappingBodyBuilder;

    public DescriptorBuilder(
        SourceProductionContext sourceContext,
        Compilation compilation,
        ClassDeclarationSyntax mapperSyntax,
        INamedTypeSymbol mapperSymbol)
    {
        _mapperSymbol = mapperSymbol;
        _context = sourceContext;
        _mapperDescriptor = new MapperDescriptor(mapperSyntax, mapperSymbol, _methodNameBuilder);
        _mappingBodyBuilder = new MappingBodyBuilder(_mappings);
        Compilation = compilation;
        WellKnownTypes = new WellKnownTypes(Compilation);
        MappingBuilder = new MappingBuilder(this, _mappings);
        ExistingTargetMappingBuilder = new ExistingTargetMappingBuilder(this, _mappings);
        MapperConfiguration = Configure();
    }

    internal IReadOnlyDictionary<Type, Attribute> DefaultConfigurations => _defaultConfigurations;

    internal Compilation Compilation { get; }

    internal WellKnownTypes WellKnownTypes { get; }

    internal ObjectFactoryCollection ObjectFactories { get; private set; } = ObjectFactoryCollection.Empty;

    public MapperAttribute MapperConfiguration { get; }

    public MappingBuilder MappingBuilder { get; }

    public ExistingTargetMappingBuilder ExistingTargetMappingBuilder { get; }

    private MapperAttribute Configure()
    {
        var mapperAttribute = AttributeDataAccessor.AccessFirstOrDefault<MapperAttribute>(Compilation, _mapperSymbol) ?? new();
        if (!_mapperSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            _mapperDescriptor.Namespace = _mapperSymbol.ContainingNamespace.ToDisplayString();
        }

        _defaultConfigurations.Add(
            typeof(MapEnumAttribute),
            new MapEnumAttribute(mapperAttribute.EnumMappingStrategy) { IgnoreCase = mapperAttribute.EnumMappingIgnoreCase });
        return mapperAttribute;
    }

    public MapperDescriptor Build()
    {
        ReserveMethodNames();
        ExtractObjectFactories();
        ExtractUserMappings();
        _mappingBodyBuilder.BuildMappingBodies();
        BuildMappingMethodNames();
        BuildReferenceHandlingParameters();
        AddMappingsToDescriptor();
        return _mapperDescriptor;
    }

    private void ExtractObjectFactories()
    {
        var ctx = new SimpleMappingBuilderContext(this);
        ObjectFactories = ObjectFactoryBuilder.ExtractObjectFactories(ctx, _mapperSymbol);
    }

    internal void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        => _context.ReportDiagnostic(Diagnostic.Create(descriptor, location ?? _mapperDescriptor.Syntax.GetLocation(), messageArgs));

    private void ExtractUserMappings()
    {
        var defaultContext = new SimpleMappingBuilderContext(this);
        foreach (var userMapping in UserMethodMappingExtractor.ExtractUserMappings(defaultContext, _mapperSymbol))
        {
            var ctx = new MappingBuilderContext(
                this,
                userMapping.SourceType,
                userMapping.TargetType,
                userMapping.Method);
            _mappings.AddMapping(userMapping);
            _mappings.EnqueueMappingToBuildBody(userMapping, ctx);
        }
    }

    private void ReserveMethodNames()
    {
        foreach (var methodSymbol in _mapperSymbol.GetAllMembers())
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
        if (!MapperConfiguration.UseReferenceHandling)
            return;

        foreach (var methodMapping in _mappings.MethodMappings)
        {
            methodMapping.EnableReferenceHandling(WellKnownTypes.IReferenceHandler);
        }
    }

    private void AddMappingsToDescriptor()
    {
        // add generated mappings to the mapper
        foreach (var mapping in _mappings.All)
        {
            _mapperDescriptor.AddTypeMapping(mapping);
        }
    }
}
