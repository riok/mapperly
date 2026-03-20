using Microsoft.CodeAnalysis;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors;

public class ParameterScope
{
    private static readonly IReadOnlyDictionary<string, MethodParameter> EmptyParameters = new Dictionary<string, MethodParameter>();

    private readonly ParameterScope? _parent;
    private readonly IReadOnlyDictionary<string, MethodParameter> _parameters;
    private readonly HashSet<string>? _usedParameters;

    public static readonly ParameterScope Empty = new([]);

    public ParameterScope(IReadOnlyCollection<MethodParameter> parameters)
    {
        if (parameters.Count == 0)
        {
            _parameters = EmptyParameters;
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
    /// Checks if all requested additional parameters can be satisfied by this scope.
    /// Matching is by name (case-insensitive). A parameter can be matched by multiple consumers.
    /// </summary>
    public bool TryMatchParameters(IReadOnlyCollection<MethodParameter> requested, out IReadOnlyList<MethodParameter> matched)
    {
        var result = new List<MethodParameter>(requested.Count);
        foreach (var param in requested)
        {
            if (!_parameters.TryGetValue(param.NormalizedName, out var scopeParam))
            {
                matched = [];
                return false;
            }
            result.Add(scopeParam);
        }
        matched = result;
        return true;
    }

    /// <summary>
    /// Checks whether all parameters of a method can be satisfied by this scope (by normalized name).
    /// Returns true for parameterless methods. A null or empty scope can only satisfy parameterless methods.
    /// </summary>
    public static bool CanSatisfyParameters(ParameterScope? scope, IMethodSymbol method) =>
        method.Parameters.Length == 0
        || (scope is { IsEmpty: false } && method.Parameters.All(p => scope._parameters.ContainsKey(NormalizeName(p.Name))));

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

        _usedParameters?.Add(NormalizeName(name));
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

    private static string NormalizeName(string name) => name.TrimStart('@');

    /// <summary>
    /// Returns parameter names that were never consumed by any consumer (for diagnostics).
    /// </summary>
    public IEnumerable<string> GetUnusedParameterNames() =>
        _usedParameters is null ? [] : _parameters.Keys.Where(k => !_usedParameters.Contains(k));
}
