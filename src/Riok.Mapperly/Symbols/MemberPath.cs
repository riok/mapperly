using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    private const string MemberAccessSeparator = ".";
    private const string NullableValueProperty = "Value";

    public MemberPath(IReadOnlyList<IMappableMember> path)
    {
        Path = path;
        FullName = string.Join(MemberAccessSeparator, Path.Select(x => x.Name));
    }

    public IReadOnlyList<IMappableMember> Path { get; }

    /// <summary>
    /// Gets the path without the very last element (the path of the object containing the <see cref="Member"/>).
    /// </summary>
    public IEnumerable<IMappableMember> ObjectPath => Path.SkipLast();

    /// <summary>
    /// Gets the last part of the path or throws if there is none.
    /// </summary>
    public IMappableMember Member => Path.Last();

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
}
