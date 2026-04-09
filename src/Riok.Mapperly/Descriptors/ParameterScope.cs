using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class ParameterScope
{
    private static readonly IReadOnlyDictionary<string, MethodParameter> _emptyParameters = new Dictionary<string, MethodParameter>();

    private readonly ParameterScope? _parent;
    private readonly IReadOnlyDictionary<string, MethodParameter> _parameters;
    private readonly HashSet<string>? _usedParameters;

    public static readonly ParameterScope Empty = new([]);

    public ParameterScope(IReadOnlyCollection<MethodParameter> parameters)
    {
        if (parameters.Count == 0)
        {
            _parameters = _emptyParameters;
            return;
        }

        _parameters = parameters.ToDictionary(p => p.NormalizedName, p => p, StringComparer.OrdinalIgnoreCase);
        _usedParameters = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a child scope that delegates to the given parent.
    /// Only the root scope (no parent) tracks and reports unused parameters.
    /// </summary>
    public ParameterScope(ParameterScope parent)
    {
        _parent = parent;
        _parameters = parent._parameters;
    }

    public bool IsRoot => _parent == null && _usedParameters != null;

    public bool IsEmpty => _parameters.Count == 0;

    public IReadOnlyDictionary<string, MethodParameter> Parameters => _parameters;

    /// <summary>
    /// Checks if all requested additional parameters can be satisfied by this scope (by normalized name).
    /// </summary>
    private bool CanMatchParameters(IReadOnlyCollection<MethodParameter> requested) =>
        requested.All(p => _parameters.ContainsKey(p.NormalizedName));

    /// <summary>
    /// Checks if all parameters of a method can be satisfied by this scope (by normalized name).
    /// </summary>
    public bool CanMatchParameters(IMethodSymbol method) =>
        method.Parameters.All(p => _parameters.ContainsKey(MethodParameter.NormalizeName(p.Name)));

    /// <summary>
    /// If the mapping is parameterized, checks if all its additional parameters can be
    /// satisfied by this scope and marks them as used. Returns false only when the mapping
    /// requires parameters that this scope cannot satisfy.
    /// </summary>
    public bool TryUseParameters(ITypeMapping? mapping)
    {
        if (mapping is not IParameterizedMapping { AdditionalSourceParameters.Count: > 0 } pm)
            return true;

        if (!CanMatchParameters(pm.AdditionalSourceParameters))
            return false;

        MarkUsed(pm.AdditionalSourceParameters);
        return true;
    }

    /// <summary>
    /// Mark a parameter as having at least one consumer (idempotent).
    /// Delegates to the root scope so usage tracking stays unified.
    /// </summary>
    public void MarkUsed(string name)
    {
        if (_parent != null)
        {
            _parent.MarkUsed(name);
            return;
        }

        _usedParameters?.Add(MethodParameter.NormalizeName(name));
    }

    /// <summary>
    /// Mark all parameters in the collection as used.
    /// </summary>
    public void MarkUsed(IEnumerable<MethodParameter> parameters)
    {
        foreach (var param in parameters)
        {
            MarkUsed(param.Name);
        }
    }

    /// <summary>
    /// Mark all named parameters as used.
    /// </summary>
    public void MarkUsed(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            MarkUsed(name);
        }
    }

    /// <summary>
    /// Returns parameter names that were never consumed by any consumer (for diagnostics).
    /// </summary>
    public IEnumerable<string> GetUnusedParameterNames() =>
        _usedParameters is null ? [] : _parameters.Keys.Where(k => !_usedParameters.Contains(k));
}
