using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Descriptors.MappingBuilder;
using Riok.Mapperly.Descriptors.TypeMappings;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors;

public class DescriptorBuilder
{
    private delegate TypeMapping? MappingBuilder(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<MappingBuilder> _mappingBuilders = new MappingBuilder[]
    {
        SpecialTypeMappingBuilder.TryBuildMapping,
        ValueTypeMappingBuilder.TryBuildMapping,
        DictionaryMappingBuilder.TryBuildMapping,
        EnumerableMappingBuilder.TryBuildMapping,
        ImplicitCastMappingBuilder.TryBuildMapping,
        ParseMappingBuilder.TryBuildMapping,
        CtorMappingBuilder.TryBuildMapping,
        EnumMappingBuilder.TryBuildMapping,
        ExplicitCastMappingBuilder.TryBuildMapping,
        ToStringMappingBuilder.TryBuildMapping,
        ObjectPropertyMappingBuilder.TryBuildMapping,
    };

    private readonly SourceProductionContext _context;
    private readonly ITypeSymbol _mapperSymbol;
    private readonly SyntaxNode _mapperSyntax;
    private readonly MapperDescriptor _mapperDescriptor;

    // default configurations
    private readonly Dictionary<Type, Attribute> _defaultConfigurations = new();

    // this includes mappings to build and already built mappings
    private readonly Dictionary<(ITypeSymbol SourceType, ITypeSymbol TargetType), TypeMapping> _mappings = new();

    // additional user defined mappings
    // (with same signature as already defined mappings but with different names)
    private readonly List<TypeMapping> _extraMappings = new();

    // queue of mappings which don't have the body built yet
    private readonly Queue<(TypeMapping, MappingBuilderContext)> _mappingsToBuildBody = new();

    public DescriptorBuilder(
        SourceProductionContext sourceContext,
        Compilation compilation,
        SyntaxNode mapperSyntax,
        ITypeSymbol mapperSymbol)
    {
        _mapperSyntax = mapperSyntax;
        _mapperSymbol = mapperSymbol;
        _context = sourceContext;
        Compilation = compilation;
        _mapperDescriptor = new MapperDescriptor(mapperSymbol.Name);
        Configure();
    }

    internal IReadOnlyDictionary<Type, Attribute> DefaultConfigurations => _defaultConfigurations;

    internal Compilation Compilation { get; }

    private void Configure()
    {
        var mapperAttribute = AttributeDataAccessor.AccessFirstOrDefault<MapperAttribute>(Compilation, _mapperSymbol) ?? new();
        if (!_mapperSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            _mapperDescriptor.Namespace = _mapperSymbol.ContainingNamespace.ToDisplayString();
        }

        _mapperDescriptor.IsAbstractClassDefinition = _mapperSyntax is not InterfaceDeclarationSyntax;
        _mapperDescriptor.Accessibility = _mapperSymbol.DeclaredAccessibility;
        _mapperDescriptor.Name = mapperAttribute.ImplementationName ?? BuildName();
        _mapperDescriptor.InstanceName = mapperAttribute.InstanceName;

        _defaultConfigurations.Add(typeof(MapEnumAttribute), new MapEnumAttribute(mapperAttribute.EnumMappingStrategy));
    }

    private string BuildName()
    {
        return !_mapperDescriptor.IsAbstractClassDefinition && _mapperSymbol.Name.StartsWith(MapperDescriptor.InterfaceNamePrefix)
            ? _mapperSymbol.Name.Substring(MapperDescriptor.InterfaceNamePrefix.Length)
            : _mapperSymbol.Name + MapperDescriptor.ImplClassNameSuffix;
    }

    public MapperDescriptor Build()
    {
        // extract mappings from declarations
        var defaultContext = new SimpleMappingBuilderContext(this);
        foreach (var userMapping in UserMethodMappingBuilder.ExtractUserMappings(defaultContext, _mapperSymbol))
        {
            AddUserMapping(userMapping);

            var ctx = new MappingBuilderContext(
                this,
                userMapping.SourceType,
                userMapping.TargetType,
                (userMapping as IHasUserSymbolMapping)?.Method);
            _mappingsToBuildBody.Enqueue((userMapping, ctx));
        }

        // build mapping bodies
        foreach (var (mapping, ctx) in _mappingsToBuildBody.DequeueAll())
        {
            BuildMappingBody(ctx, mapping);
        }

        // add generated mappings to the mapper
        foreach (var mapping in _mappings.Values)
        {
            _mapperDescriptor.AddTypeMapping(mapping);
        }

        // add extra mappings to the mapper
        foreach (var extraMapping in _extraMappings)
        {
            _mapperDescriptor.AddTypeMapping(extraMapping);
        }

        return _mapperDescriptor;
    }

    internal TypeMapping? FindOrBuildMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (FindMapping(sourceType, targetType) is { } foundMapping)
            return foundMapping;

        if (TryBuildNewMapping(null, sourceType, targetType) is not { } mapping)
            return null;

        AddMapping(mapping);
        return mapping;
    }

    public TypeMapping? TryBuildNewMapping(
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
        => _context.ReportDiagnostic(Diagnostic.Create(descriptor, location ?? _mapperSyntax.GetLocation(), messageArgs));

    private void BuildMappingBody(MappingBuilderContext ctx, TypeMapping typeMapping)
    {
        switch (typeMapping)
        {
            case ObjectPropertyMapping mapping:
                ObjectPropertyMappingBuilder.BuildMappingBody(ctx, mapping);
                break;
            case UserDefinedNewInstanceMethodMapping mapping:
                UserMethodMappingBuilder.BuildMappingBody(ctx, mapping);
                break;
        }
    }

    private void AddMapping(TypeMapping mapping)
        => _mappings.Add((mapping.SourceType, mapping.TargetType), mapping);

    private void AddUserMapping(TypeMapping mapping)
    {
        if (mapping.CallableByOtherMappings && FindMapping(mapping.SourceType, mapping.TargetType) is null)
        {
            AddMapping(mapping);
            return;
        }

        _extraMappings.Add(mapping);
    }

    private TypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _mappings.TryGetValue((sourceType, targetType), out var mapping);
        return mapping;
    }
}
