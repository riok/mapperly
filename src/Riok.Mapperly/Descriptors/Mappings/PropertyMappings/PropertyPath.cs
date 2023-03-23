using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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
    /// Gets the type of the <see cref="Member"/>. If any part of the path is nullable, this type will be nullable too.
    /// </summary>
    public ITypeSymbol MemberType =>
        IsAnyNullable()
            ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated)
            : Member.Type;

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

    public bool IsAnyObjectPathNullable()
        => ObjectPath.Any(p => p.IsNullable());

    public ExpressionSyntax BuildAccess(
        ExpressionSyntax? baseAccess,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false,
        bool skipTrailingNonNullable = false)
    {
        var path = skipTrailingNonNullable
            ? PathWithoutTrailingNonNullable()
            : Path;

        if (baseAccess == null)
        {
            baseAccess = IdentifierName(path.First().Name);
            path = path.Skip(1);
        }

        if (nullConditional)
        {
            return path.AggregateWithPrevious(
                baseAccess,
                (expr, prevProp, prop) => prevProp?.IsNullable() == true
                    ? ConditionalAccess(expr, prop.Name)
                    : MemberAccess(expr, prop.Name));
        }

        if (addValuePropertyOnNullable)
        {
            return path.Aggregate(baseAccess, (a, b) => b.Type.IsNullableValueType()
                ? MemberAccess(MemberAccess(a, b.Name), NullableValueProperty)
                : MemberAccess(a, b.Name));
        }

        return path.Aggregate(baseAccess, (a, b) => MemberAccess(a, b.Name));
    }

    /// <summary>
    /// Builds a condition (the resulting expression evaluates to a boolean)
    /// whether the path is non-null.
    /// </summary>
    /// <param name="baseAccess">The base access to access the property or <c>null</c>.</param>
    /// <returns><c>null</c> if no part of the path is nullable or the condition which needs to be true, that the path cannot be <c>null</c>.</returns>
    public ExpressionSyntax? BuildNonNullConditionWithoutConditionalAccess(ExpressionSyntax? baseAccess)
    {
        var path = PathWithoutTrailingNonNullable();
        ExpressionSyntax? condition = null;
        var access = baseAccess;
        if (access == null)
        {
            access = IdentifierName(path.First().Name);
            path = path.Skip(1);
        }

        foreach (var pathPart in path)
        {
            access = MemberAccess(access, pathPart.Name);

            if (!pathPart.IsNullable())
                continue;

            condition = And(condition, IsNotNull(access));
        }

        return condition;
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

    public static bool TryFind(
        ITypeSymbol type,
        IEnumerable<IEnumerable<string>> pathCandidates,
        IReadOnlyCollection<string> ignoredNames,
        [NotNullWhen(true)] out PropertyPath? propertyPath)
        => TryFind(type, pathCandidates, ignoredNames, StringComparer.Ordinal, out propertyPath);

    public static bool TryFind(
        ITypeSymbol type,
        IEnumerable<IEnumerable<string>> pathCandidates,
        IReadOnlyCollection<string> ignoredNames,
        IEqualityComparer<string> comparer,
        [NotNullWhen(true)] out PropertyPath? propertyPath)
    {
        foreach (var pathCandidate in FindCandidates(type, pathCandidates, comparer))
        {
            if (ignoredNames.Contains(pathCandidate.Path.First().Name))
                continue;

            propertyPath = pathCandidate;
            return true;
        }

        propertyPath = null;
        return false;
    }

    public static bool TryFind(
        ITypeSymbol type,
        IReadOnlyCollection<string> path,
        [NotNullWhen(true)] out PropertyPath? propertyPath)
        => TryFind(type, path, StringComparer.Ordinal, out propertyPath);

    private static IEnumerable<PropertyPath> FindCandidates(
        ITypeSymbol type,
        IEnumerable<IEnumerable<string>> pathCandidates,
        IEqualityComparer<string> comparer)
    {
        foreach (var pathCandidate in pathCandidates)
        {
            if (TryFind(type, pathCandidate.ToList(), comparer, out var propertyPath))
                yield return propertyPath;
        }
    }

    private static bool TryFind(
        ITypeSymbol type,
        IReadOnlyCollection<string> path,
        IEqualityComparer<string> comparer,
        [NotNullWhen(true)] out PropertyPath? propertyPath)
    {
        var foundPath = Find(type, path, comparer).ToList();
        if (foundPath.Count != path.Count)
        {
            propertyPath = null;
            return false;
        }

        propertyPath = new(foundPath);
        return true;
    }

    private static IEnumerable<IPropertySymbol> Find(
        ITypeSymbol type,
        IEnumerable<string> path,
        IEqualityComparer<string> comparer)
    {
        foreach (var name in path)
        {
            if (FindProperty(type, name, comparer) is not { } property)
                break;

            type = property.Type;
            yield return property;
        }
    }

    private static IPropertySymbol? FindProperty(
        ITypeSymbol type,
        string name,
        IEqualityComparer<string> comparer)
    {
        return type.GetAllMembers(name, comparer)
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => !p.IsStatic);
    }
}
