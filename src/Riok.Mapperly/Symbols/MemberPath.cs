using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// Represents a (possibly empty) list of members to access a certain member.
/// E.g. A.B.C
/// </summary>
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

    /// <summary>
    /// Builds a condition (the resulting expression evaluates to a boolean)
    /// whether the path is non-null.
    /// </summary>
    /// <param name="baseAccess">The base access to access the member or <c>null</c>.</param>
    /// <returns><c>null</c> if no part of the path is nullable or the condition which needs to be true, that the path cannot be <c>null</c>.</returns>
    public ExpressionSyntax? BuildNonNullConditionWithoutConditionalAccess(ExpressionSyntax? baseAccess)
    {
        var nullablePath = PathWithoutTrailingNonNullable();
        ExpressionSyntax? condition = null;
        var access = baseAccess;
        if (access == null)
        {
            access = IdentifierName(nullablePath.First().Name);
            nullablePath = nullablePath.Skip(1);
        }

        foreach (var pathPart in nullablePath)
        {
            access = MemberAccess(access, pathPart.Name);

            if (!pathPart.IsNullable)
                continue;

            condition = And(condition, IsNotNull(access));
        }

        return condition;
    }

    public static MemberPath Create(ITypeSymbol rootType, IReadOnlyList<IMappableMember> path)
    {
        if (path.Count == 0)
        {
            return new EmptyMemberPath(rootType);
        }

        return new NonEmptyMemberPath(rootType, path);
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

    private bool Equals(MemberPath other) =>
        RootType.Equals(other.RootType, SymbolEqualityComparer.IncludeNullability) && Path.SequenceEqual(other.Path);

    public abstract string ToDisplayString(bool includeMemberType = true);
}
