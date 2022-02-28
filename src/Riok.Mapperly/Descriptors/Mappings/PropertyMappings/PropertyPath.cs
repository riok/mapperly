using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.PropertyMappings;

/// <summary>
/// Represents a set of properties to access a certain property.
/// Eg. A.B.C
/// </summary>
[DebuggerDisplay("{FullName}")]
public class PropertyPath
{
    internal const string PropertyAccessSeparator = ".";
    private const string NullableValueProperty = "Value";

    private IPropertySymbol? _member;

    public PropertyPath(IReadOnlyCollection<IPropertySymbol> path)
    {
        Path = path;
        FullName = string.Join(PropertyAccessSeparator, Path.Select(x => x.Name));
    }

    public IReadOnlyCollection<IPropertySymbol> Path { get; }

    /// <summary>
    /// Gets the path without the very last element (the path of the object containing the <see cref="Member"/>).
    /// </summary>
    public IEnumerable<IPropertySymbol> ObjectPath => Path.SkipLast();

    /// <summary>
    /// Gets the last part of the path or throws if there is none.
    /// </summary>
    public IPropertySymbol Member
    {
        get => _member ??= Path.Last();
    }

    /// <summary>
    /// Gets the full name of the path (eg. A.B.C).
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Builds a property path skipping trailing path items which are non nullable.
    /// </summary>
    /// <returns>The built path.</returns>
    public IEnumerable<IPropertySymbol> PathWithoutTrailingNonNullable()
        => Path.Reverse().SkipWhile(x => !x.IsNullable()).Reverse();

    /// <summary>
    /// Returns an element for each nullable sub-path of the <see cref="ObjectPath"/>.
    /// If the <see cref="Member"/> is nullable, the entire <see cref="Path"/> is not returned.
    /// </summary>
    /// <returns>All nullable sub-paths of the <see cref="ObjectPath"/>.</returns>
    public IEnumerable<IReadOnlyCollection<IPropertySymbol>> ObjectPathNullableSubPaths()
    {
        var pathParts = new List<IPropertySymbol>(Path.Count);
        foreach (var pathPart in ObjectPath)
        {
            pathParts.Add(pathPart);
            if (!pathPart.IsNullable())
                continue;

            yield return pathParts;
        }
    }

    public bool IsAnyNullable()
        => Path.Any(p => p.IsNullable());

    public ExpressionSyntax BuildAccess(
        ExpressionSyntax baseAccess,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false,
        bool skipTrailingNonNullable = false)
    {
        if (!nullConditional)
        {
            if (addValuePropertyOnNullable)
            {
                return Path.Aggregate(baseAccess, (a, b) => b.Type.IsNullableValueType()
                    ? MemberAccess(MemberAccess(a, b.Name), NullableValueProperty)
                    : MemberAccess(a, b.Name));
            }

            return Path.Aggregate(baseAccess, (a, b) => MemberAccess(a, b.Name));
        }

        var path = skipTrailingNonNullable
            ? PathWithoutTrailingNonNullable()
            : Path;

        return path.AggregateWithPrevious(
            baseAccess,
            (expr, prevProp, prop) => prevProp?.IsNullable() == true
                ? ConditionalAccess(expr, prop.Name)
                : MemberAccess(expr, prop.Name));
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((PropertyPath)obj);
    }

    public override int GetHashCode()
    {
        var hc = 0;
        foreach (var item in Path)
        {
            hc ^= SymbolEqualityComparer.Default.GetHashCode(item);
        }

        return hc;
    }

    public static bool operator ==(PropertyPath? left, PropertyPath? right)
        => Equals(left, right);

    public static bool operator !=(PropertyPath? left, PropertyPath? right)
        => !Equals(left, right);

    private bool Equals(PropertyPath other)
        => Path.SequenceEqual(other.Path, SymbolEqualityComparer.IncludeNullability);
}
