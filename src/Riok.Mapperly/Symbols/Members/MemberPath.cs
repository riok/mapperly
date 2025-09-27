using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Configuration.PropertyReferences;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// Represents a (possibly empty) list of members to access a certain member.
/// E.g. A.B.C
/// </summary>
[DebuggerDisplay("{ToDebugString()}")]
public abstract class MemberPath(ITypeSymbol rootType, IReadOnlyList<IMappableMember> path)
{
    protected const string MemberAccessSeparator = ".";

    public IReadOnlyList<IMappableMember> Path { get; } = path;

    /// <summary>
    /// Gets the path without the very last element (the path of the object containing the <see cref="Member"/>).
    /// </summary>
    public IEnumerable<IMappableMember> ObjectPath => Path.SkipLast();

    public ITypeSymbol RootType { get; } = rootType;

    /// <summary>
    /// Gets the last part of the path or <see langword="null"/> if there is none.
    /// </summary>
    public abstract IMappableMember? Member { get; }

    /// <summary>
    /// Gets the type of the total path, e.g. that of the <see cref="Member"/> if it exists, or the <see cref="RootType"/> otherwise.
    /// If any part of the path is nullable, this type will be nullable too.
    /// </summary>
    public abstract ITypeSymbol MemberType { get; }

    /// <summary>
    /// Gets the full name of the path (e.g. A.B.C).
    /// </summary>
    public string FullName { get; } = string.Join(MemberAccessSeparator, path.Select(x => x.Name));

    /// <summary>
    /// Builds a member path skipping trailing path items which are non-nullable.
    /// </summary>
    /// <returns>The built path.</returns>
    public IEnumerable<IMappableMember> PathWithoutTrailingNonNullable() => Path.Reverse().SkipWhile(x => !x.IsNullable).Reverse();

    /// <summary>
    /// Returns an element for each nullable sub-path of the <see cref="ObjectPath"/>.
    /// If the <see cref="Member"/> is nullable, the entire <see cref="Path"/> is not returned.
    /// </summary>
    /// <returns>All nullable sub-paths of the <see cref="ObjectPath"/>.</returns>
    public IEnumerable<IReadOnlyList<IMappableMember>> ObjectPathNullableSubPaths()
    {
        var pathParts = new List<IMappableMember>(Path.Count);
        foreach (var pathPart in ObjectPath)
        {
            pathParts.Add(pathPart);
            if (!pathPart.IsNullable)
                continue;

            yield return pathParts.ToArray();
        }
    }

    public bool IsAnyNullable() => Path.Any(p => p.IsNullable);

    public bool IsAnyObjectPathNullable() => ObjectPath.Any(p => p.IsNullable);

    public MemberPathGetter BuildGetter(SimpleMappingBuilderContext ctx) => MemberPathGetter.Build(ctx, this);

    public static MemberPath Create(ITypeSymbol rootType, IReadOnlyList<IMappableMember> path)
    {
        if (path.Count == 0)
        {
            return new EmptyMemberPath(rootType);
        }

        return new NonEmptyMemberPath(rootType, path);
    }

    public bool Equals(IMemberPathConfiguration path)
    {
        if (path.PathCount != Path.Count)
            return false;

        foreach (var (pathSegment1, pathSegment2) in Path.Zip(path.MemberNames))
        {
            if (!string.Equals(pathSegment1.Name, pathSegment2, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (MemberPath)obj;
        return RootType.Equals(other.RootType, SymbolEqualityComparer.IncludeNullability) && Path.SequenceEqual(other.Path);
    }

    public override int GetHashCode()
    {
        var hc = 0;
        foreach (var item in Path)
        {
            hc ^= item.GetHashCode();
        }

        return hc;
    }

    public string ToDebugString() => ToDisplayString();

    public abstract string ToDisplayString(bool includeRootType = true, bool includeMemberType = true);
}
