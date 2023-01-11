using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilder;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class DescriptorBuilder
{
    private delegate ITypeMapping? MappingBuilder(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<MappingBuilder> _mappingBuilders = new MappingBuilder[]
    {
        NullableMappingBuilder.TryBuildMapping,
        SpecialTypeMappingBuilder.TryBuildMapping,
        DirectAssignmentMappingBuilder.TryBuildMapping,
        DictionaryMappingBuilder.TryBuildMapping,
        EnumerableMappingBuilder.TryBuildMapping,
        ImplicitCastMappingBuilder.TryBuildMapping,
        ParseMappingBuilder.TryBuildMapping,
        CtorMappingBuilder.TryBuildMapping,
        StringToEnumMappingBuilder.TryBuildMapping,
        EnumToStringMappingBuilder.TryBuildMapping,
        EnumMappingBuilder.TryBuildMapping,
        ExplicitCastMappingBuilder.TryBuildMapping,
        ToStringMappingBuilder.TryBuildMapping,
        NewInstanceObjectPropertyMappingBuilder.TryBuildMapping,
    };

    private readonly SourceProductionContext _context;
    private readonly ITypeSymbol _mapperSymbol;
    private readonly MapperDescriptor _mapperDescriptor;

    // default configurations, used a configuration is needed but no configuration is provided by the user
    // these are the default configurations registered for each configuration attribute.
    // Usually these are derived from the mapper attribute or default values.
    private readonly Dictionary<Type, Attribute> _defaultConfigurations = new();

    // queue of mappings which don't have the body built yet
    private readonly Queue<(ITypeMapping, MappingBuilderContext)> _mappingsToBuildBody = new();

    private readonly MappingCollection _mappings = new();

    private readonly MethodNameBuilder _methodNameBuilder = new();

    public DescriptorBuilder(
        SourceProductionContext sourceContext,
        Compilation compilation,
        ClassDeclarationSyntax mapperSyntax,
        ITypeSymbol mapperSymbol)
    {
        _mapperSymbol = mapperSymbol;
        _context = sourceContext;
        Compilation = compilation;
        WellKnownTypes = new WellKnownTypes(Compilation);
        _mapperDescriptor = new MapperDescriptor(mapperSyntax, mapperSymbol.IsStatic);
        MapperConfiguration = Configure();
    }

    internal IReadOnlyDictionary<Type, Attribute> DefaultConfigurations => _defaultConfigurations;

    internal Compilation Compilation { get; }

    internal WellKnownTypes WellKnownTypes { get; }

    internal ObjectFactoryCollection ObjectFactories { get; private set; } = ObjectFactoryCollection.Empty;

    public MapperAttribute MapperConfiguration { get; }

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
        BuildMappingBodies();
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
    public ITypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        => _mappings.FindMapping(sourceType, targetType);

    public ITypeMapping? FindOrBuildMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (_mappings.FindMapping(sourceType, targetType) is { } foundMapping)
            return foundMapping;

        if (BuildDelegateMapping(null, sourceType, targetType) is not { } mapping)
            return null;

        _mappings.AddMapping(mapping);
        return mapping;
    }

    public ITypeMapping? BuildMappingWithUserSymbol(
        ISymbol userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (BuildDelegateMapping(userSymbol, sourceType, targetType) is not { } mapping)
            return null;

        _mappings.AddMapping(mapping);
        return mapping;
    }

    public ITypeMapping? BuildDelegateMapping(
        ISymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        var ctx = new MappingBuilderContext(this, sourceType, targetType, userSymbol);
        foreach (var mappingBuilder in _mappingBuilders)
        {
            if (mappingBuilder(ctx) is { } mapping)
            {
                _mappingsToBuildBody.Enqueue((mapping, ctx));
                return mapping;
            }
        }

        return null;
    }

    internal void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object[] messageArgs)
        => _context.ReportDiagnostic(Diagnostic.Create(descriptor, location ?? _mapperDescriptor.Syntax.GetLocation(), messageArgs));

    private void ExtractUserMappings()
    {
        var defaultContext = new SimpleMappingBuilderContext(this);
        foreach (var userMapping in UserMethodMappingBuilder.ExtractUserMappings(defaultContext, _mapperSymbol))
        {
            _mappings.AddMapping(userMapping);

            var ctx = new MappingBuilderContext(
                this,
                userMapping.SourceType,
                userMapping.TargetType,
                userMapping.Method);
            _mappingsToBuildBody.Enqueue((userMapping, ctx));
        }
    }

    private void ReserveMethodNames()
    {
        foreach (var methodSymbol in _mapperSymbol.GetAllMembers().OfType<IMethodSymbol>())
        {
            _methodNameBuilder.Reserve(methodSymbol.Name);
        }
    }

    private void BuildMappingBodies()
    {
        foreach (var (typeMapping, ctx) in _mappingsToBuildBody.DequeueAll())
        {
            switch (typeMapping)
            {
                case NewInstanceObjectPropertyMapping mapping:
                    NewInstanceObjectPropertyMappingBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case ObjectPropertyMapping mapping:
                    ObjectPropertyMappingBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedNewInstanceMethodMapping mapping:
                    UserMethodMappingBuilder.BuildMappingBody(ctx, mapping);
                    break;
            }
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
