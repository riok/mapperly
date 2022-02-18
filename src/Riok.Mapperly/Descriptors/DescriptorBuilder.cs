using Microsoft.CodeAnalysis;
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
        ObjectPropertyMappingBuilder.TryBuildMapping,
    };

    private readonly SourceProductionContext _context;
    private readonly ITypeSymbol _mapperSymbol;
    private readonly SyntaxNode _mapperSyntax;
    private readonly MapperDescriptor _mapperDescriptor;

    // default configurations, used a configuration is needed but no configuration is provided by the user
    // these are the default configurations registered for each configuration attribute.
    // Usually these are derived from the mapper attribute or default values.
    private readonly Dictionary<Type, Attribute> _defaultConfigurations = new();

    // this includes mappings to build and already built mappings
    private readonly Dictionary<(ITypeSymbol SourceType, ITypeSymbol TargetType), TypeMapping> _mappings = new(new MappingTupleEqualityComparer());

    // additional user defined mappings
    // (with same signature as already defined mappings but with different names)
    private readonly List<TypeMapping> _extraMappings = new();

    // queue of mappings which don't have the body built yet
    private readonly Queue<(TypeMapping, MappingBuilderContext)> _mappingsToBuildBody = new();

    private readonly MethodNameBuilder _methodNameBuilder = new();

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
        MapperConfiguration = Configure();
    }

    internal IReadOnlyDictionary<Type, Attribute> DefaultConfigurations => _defaultConfigurations;

    internal Compilation Compilation { get; }

    public MapperAttribute MapperConfiguration { get; }

    private MapperAttribute Configure()
    {
        var mapperAttribute = AttributeDataAccessor.AccessFirstOrDefault<MapperAttribute>(Compilation, _mapperSymbol) ?? new();
        if (!_mapperSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            _mapperDescriptor.Namespace = _mapperSymbol.ContainingNamespace.ToDisplayString();
        }

        _mapperDescriptor.Accessibility = _mapperSymbol.DeclaredAccessibility;

        _defaultConfigurations.Add(
            typeof(MapEnumAttribute),
            new MapEnumAttribute(mapperAttribute.EnumMappingStrategy) { IgnoreCase = mapperAttribute.EnumMappingIgnoreCase });
        return mapperAttribute;
    }

    public MapperDescriptor Build()
    {
        ReserveMethodNames();
        ExtractUserMappings();
        BuildMappingBodies();
        BuildMappingMethodNames();
        AddMappingsToDescriptor();
        return _mapperDescriptor;
    }

    public TypeMapping? FindOrBuildMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (FindMapping(sourceType, targetType) is { } foundMapping)
            return foundMapping;

        if (BuildDelegateMapping(null, sourceType, targetType) is not { } mapping)
            return null;

        AddMapping(mapping);
        return mapping;
    }

    public TypeMapping? FindMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        _mappings.TryGetValue((sourceType, targetType), out var mapping);
        return mapping;
    }

    public TypeMapping? FindOrBuildDelegateMapping(
        ISymbol? userSymbol,
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
    {
        if (FindMapping(sourceType, targetType) is { } foundMapping)
            return foundMapping;

        return BuildDelegateMapping(userSymbol, sourceType, targetType);
    }

    public TypeMapping? BuildDelegateMapping(
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

    private void ExtractUserMappings()
    {
        var defaultContext = new SimpleMappingBuilderContext(this);
        foreach (var userMapping in UserMethodMappingBuilder.ExtractUserMappings(defaultContext, _mapperSymbol))
        {
            AddUserMapping(userMapping);

            var ctx = new MappingBuilderContext(
                this,
                userMapping.SourceType,
                userMapping.TargetType,
                (userMapping as IUserMapping)?.Method);
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
                case ObjectPropertyMapping mapping:
                    ObjectPropertyMappingBuilder.BuildMappingBody(ctx, mapping);
                    break;
                case UserDefinedNewInstanceMethodMapping mapping:
                    UserMethodMappingBuilder.BuildMappingBody(ctx, mapping);
                    break;
            }
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

    private void BuildMappingMethodNames()
    {
        foreach (var methodMapping in _mappings.Values.Concat(_extraMappings).OfType<MethodMapping>())
        {
            methodMapping.SetMethodNameIfNeeded(_methodNameBuilder.Build);
        }
    }

    private void AddMappingsToDescriptor()
    {
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
    }

    private class MappingTupleEqualityComparer : IEqualityComparer<(ITypeSymbol Source, ITypeSymbol Target)>
    {
        public bool Equals((ITypeSymbol Source, ITypeSymbol Target) x, (ITypeSymbol Source, ITypeSymbol Target) y)
        {
            return Equals(x.Source, y.Source)
                && Equals(x.Target, y.Target);
        }

        public int GetHashCode((ITypeSymbol Source, ITypeSymbol Target) obj)
        {
            unchecked
            {
                return (SymbolEqualityComparer.Default.GetHashCode(obj.Source) * 397) ^ SymbolEqualityComparer.Default.GetHashCode(obj.Target);
            }
        }

        private bool Equals(ITypeSymbol x, ITypeSymbol y)
            => SymbolEqualityComparer.IncludeNullability.Equals(x, y);
    }
}
