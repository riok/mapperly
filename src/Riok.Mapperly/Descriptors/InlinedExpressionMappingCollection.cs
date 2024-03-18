using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;

namespace Riok.Mapperly.Descriptors;

public class InlinedExpressionMappingCollection
{
    // use the MappingCollection to re-use
    // the mapping default logic.
    // Added mapping bodies are never built
    // and method mappings are not added to the generated mapper.
    private readonly MappingCollection _delegate = new();

    private readonly Dictionary<string, INewInstanceMapping> _namedInlinedMappings = new();
    private readonly Dictionary<TypeMappingKey, INewInstanceMapping> _inlinedMappings = new();

    public INewInstanceMapping? FindNamed(string name, out bool ambiguousName, out bool isInlined)
    {
        if (_namedInlinedMappings.TryGetValue(name, out var inlinedMapping))
        {
            isInlined = true;

            // FindNamed was at least once called for this name
            // therefore a diagnostic should have been reported already
            // if this is an ambiguous name
            ambiguousName = false;
            return inlinedMapping;
        }

        isInlined = false;
        return _delegate.FindNamedNewInstanceMapping(name, out ambiguousName);
    }

    public INewInstanceMapping? Find(TypeMappingKey mappingKey, out bool isInlined)
    {
        if (_inlinedMappings.TryGetValue(mappingKey, out var inlinedMapping))
        {
            isInlined = true;
            return inlinedMapping;
        }

        isInlined = false;
        return _delegate.FindNewInstanceMapping(mappingKey);
    }

    public INewInstanceUserMapping? FindNewInstanceUserMapping(IMethodSymbol method, out bool isInlined)
    {
        var mapping = _delegate.FindNewInstanceUserMapping(method);
        if (mapping == null)
        {
            isInlined = false;
            return null;
        }

        if (_inlinedMappings.TryGetValue(new TypeMappingKey(mapping), out var inlinedMapping))
        {
            isInlined = true;
            return inlinedMapping as INewInstanceUserMapping;
        }

        isInlined = false;
        return mapping;
    }

    public void AddMapping(INewInstanceMapping mapping, TypeMappingConfiguration config)
    {
        var result = _delegate.AddNewInstanceMapping(mapping, config);

        // only set it as inlined if it was added as default mapping
        if (result == MappingCollectionAddResult.Added)
        {
            SetInlinedMapping(new TypeMappingKey(mapping, config), mapping);
        }
    }

    public void SetInlinedMapping(TypeMappingKey mappingKey, INewInstanceMapping mapping) => _inlinedMappings[mappingKey] = mapping;

    public void SetInlinedMapping(string name, INewInstanceMapping mapping) => _namedInlinedMappings[name] = mapping;

    public void AddUserMapping(IUserMapping mapping, string? name) => _delegate.AddUserMapping(mapping, name);
}
