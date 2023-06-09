using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// Represents a set of members to access a certain member.
/// Eg. A.B.C
/// </summary>
[DebuggerDisplay("{FullName}")]
public class MemberPath
{
    internal const string MemberAccessSeparator = ".";
    private const string NullableValueProperty = "Value";

    private IMappableMember? _member;

    public MemberPath(IReadOnlyCollection<IMappableMember> path)
    {
        Path = path;
        FullName = string.Join(MemberAccessSeparator, Path.Select(x => x.Name));
    }

    public IReadOnlyCollection<IMappableMember> Path { get; }

    /// <summary>
    /// Gets the path without the very last element (the path of the object containing the <see cref="Member"/>).
    /// </summary>
    public IEnumerable<IMappableMember> ObjectPath => Path.SkipLast();

    /// <summary>
    /// Gets the last part of the path or throws if there is none.
    /// </summary>
    public IMappableMember Member
    {
        get => _member ??= Path.Last();
    }

    /// <summary>
    /// Gets the type of the <see cref="Member"/>. If any part of the path is nullable, this type will be nullable too.
    /// </summary>
    public ITypeSymbol MemberType => IsAnyNullable() ? Member.Type.WithNullableAnnotation(NullableAnnotation.Annotated) : Member.Type;

    /// <summary>
    /// Gets the full name of the path (eg. A.B.C).
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Builds a member path skipping trailing path items which are non nullable.
    /// </summary>
    /// <returns>The built path.</returns>
    public IEnumerable<IMappableMember> PathWithoutTrailingNonNullable() => Path.Reverse().SkipWhile(x => !x.IsNullable).Reverse();

    /// <summary>
    /// Returns an element for each nullable sub-path of the <see cref="ObjectPath"/>.
    /// If the <see cref="Member"/> is nullable, the entire <see cref="Path"/> is not returned.
    /// </summary>
    /// <returns>All nullable sub-paths of the <see cref="ObjectPath"/>.</returns>
    public IEnumerable<IReadOnlyCollection<IMappableMember>> ObjectPathNullableSubPaths()
    {
        var pathParts = new List<IMappableMember>(Path.Count);
        foreach (var pathPart in ObjectPath)
        {
            pathParts.Add(pathPart);
            if (!pathPart.IsNullable)
                continue;

            yield return pathParts;
        }
    }

    public bool IsAnyNullable() => Path.Any(p => p.IsNullable);

    public bool IsAnyObjectPathNullable() => ObjectPath.Any(p => p.IsNullable);

    public ExpressionSyntax BuildAccess(
        ExpressionSyntax? baseAccess,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false,
        bool skipTrailingNonNullable = false
    )
    {
        var path = skipTrailingNonNullable ? PathWithoutTrailingNonNullable() : Path;

        if (baseAccess == null)
        {
            baseAccess = IdentifierName(path.First().Name);
            path = path.Skip(1);
        }

        if (nullConditional)
        {
            return path.AggregateWithPrevious(
                baseAccess,
                (expr, prevProp, prop) => prevProp?.IsNullable == true ? ConditionalAccess(expr, prop.Name) : MemberAccess(expr, prop.Name)
            );
        }

        if (addValuePropertyOnNullable)
        {
            return path.Aggregate(
                baseAccess,
                (a, b) =>
                    b.Type.IsNullableValueType() ? MemberAccess(MemberAccess(a, b.Name), NullableValueProperty) : MemberAccess(a, b.Name)
            );
        }

        return path.Aggregate(baseAccess, (a, b) => MemberAccess(a, b.Name));
    }

    /// <summary>
    /// Builds a condition (the resulting expression evaluates to a boolean)
    /// whether the path is non-null.
    /// </summary>
    /// <param name="baseAccess">The base access to access the member or <c>null</c>.</param>
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

            if (!pathPart.IsNullable)
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

        return Equals((MemberPath)obj);
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

    public static bool operator ==(MemberPath? left, MemberPath? right) => Equals(left, right);

    public static bool operator !=(MemberPath? left, MemberPath? right) => !Equals(left, right);

    private bool Equals(MemberPath other) => Path.SequenceEqual(other.Path);

    public static bool TryFind(
        ITypeSymbol type,
        string source,
        IReadOnlyCollection<string> ignoredNames,
        [NotNullWhen(true)] out MemberPath? memberPath
    ) => TryFind(type, source, ignoredNames, StringComparison.Ordinal, out memberPath);

    public static bool TryFind(
        ITypeSymbol type,
        string source,
        IReadOnlyCollection<string> ignoredNames,
        StringComparison comparer,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        foreach (var pathCandidate in FindCandidates(type, source, comparer))
        {
            if (ignoredNames.Contains(pathCandidate.Path.First().Name))
                continue;

            memberPath = pathCandidate;
            return true;
        }

        memberPath = null;
        return false;
    }

    public static bool TryFind(ITypeSymbol type, IReadOnlyCollection<string> path, [NotNullWhen(true)] out MemberPath? memberPath) =>
        TryFind(type, path, StringComparer.Ordinal, out memberPath);

    private static IEnumerable<MemberPath> FindCandidates(ITypeSymbol type, string source, StringComparison comparer)
    {
        if (source.Length == 0)
            yield break;

        // try full string
        if (FindMember(type, source.AsSpan(), comparer) is { } fullMember)
        {
            yield return new MemberPath(new[] { fullMember });
        }

        var indices = MemberPathCandidateBuilder.GetPascalCaseSplitIndices(source).ToArray();

        var final = new IMappableMember[32];
        // try all permutations, skipping the first because the full string is already yielded
        var permutationsCount = 1 << indices.Length;

        for (var i = 1; i < permutationsCount; i++)
        {
            if (TryBuildMemberPath(type, source, comparer, indices, i, final) is { } memberPath)
            {
                yield return memberPath;
            }
        }
    }

    private static MemberPath? TryBuildMemberPath(
        ITypeSymbol type,
        string source,
        StringComparison comparer,
        int[] indices,
        int i,
        IMappableMember[] final
    )
    {
        var pos = 0;
        var lastSplitIndex = 0;
        var currentSplitPosition = 1;
        foreach (var splitIndex in indices)
        {
            if ((i & currentSplitPosition) == currentSplitPosition)
            {
                var slice = source.AsSpan().Slice(lastSplitIndex, splitIndex - lastSplitIndex);
                if (FindMember(type, slice, comparer) is not { } member)
                {
                    return null;
                }

                final[pos] = member;
                pos++;
                type = member.Type;
                lastSplitIndex = splitIndex;
            }

            currentSplitPosition <<= 1;
        }

        if (lastSplitIndex < source.Length)
        {
            var slice = source.AsSpan().Slice(lastSplitIndex);
            if (FindMember(type, slice, comparer) is not { } member)
            {
                return null;
            }

            final[pos] = member;
            pos++;
        }

        return new MemberPath(final.Take(pos).ToArray());
    }

    private static bool TryFind(
        ITypeSymbol type,
        IReadOnlyCollection<string> path,
        IEqualityComparer<string> comparer,
        [NotNullWhen(true)] out MemberPath? memberPath
    )
    {
        var foundPath = Find(type, path, comparer).ToList();
        if (foundPath.Count != path.Count)
        {
            memberPath = null;
            return false;
        }

        memberPath = new(foundPath);
        return true;
    }

    private static IEnumerable<IMappableMember> Find(ITypeSymbol type, IEnumerable<string> path, IEqualityComparer<string> comparer)
    {
        foreach (var name in path)
        {
            if (FindMember(type, name, comparer) is not { } member)
                break;

            type = member.Type;
            yield return member;
        }
    }

    private static IMappableMember? FindMember(ITypeSymbol type, string name, IEqualityComparer<string> comparer)
    {
        return type.GetMappableMembers(name, comparer).FirstOrDefault();
    }

    private static IMappableMember? FindMember(ITypeSymbol type, ReadOnlySpan<char> name, StringComparison comparer)
    {
        return type.GetMappableMembers(name, comparer);
    }
}
