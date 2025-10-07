using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Helpers;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;
using MemberGetterPair = (Riok.Mapperly.Symbols.Members.IMappableMember Member, Riok.Mapperly.Symbols.Members.IMemberGetter Getter);

namespace Riok.Mapperly.Symbols.Members;

/// <summary>
/// A getter for a <see cref="MemberPath"/>.
/// </summary>
[DebuggerDisplay("{MemberPath}")]
public class MemberPathGetter
{
    private const string NullableValueProperty = nameof(Nullable<>.Value);

    public MemberPath MemberPath { get; }

    private readonly IReadOnlyCollection<MemberGetterPair> _path;

    private MemberPathGetter(MemberPath memberPath, IReadOnlyCollection<MemberGetterPair> path)
    {
        _path = path;
        MemberPath = memberPath;
    }

    public static MemberPathGetter Build(SimpleMappingBuilderContext ctx, MemberPath path)
    {
        var getterPath = path.Path.Select(x => (x, x.BuildGetter(ctx.UnsafeAccessorContext))).ToList();
        return new MemberPathGetter(path, getterPath);
    }

    [return: NotNullIfNotNull(nameof(baseAccess))]
    public ExpressionSyntax? BuildAccess(
        ExpressionSyntax? baseAccess,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false,
        bool skipTrailingNonNullable = false
    )
    {
        var path = skipTrailingNonNullable ? PathWithoutTrailingNonNullable() : _path;
        return BuildAccess(baseAccess, path, addValuePropertyOnNullable, nullConditional);
    }

    [return: NotNullIfNotNull(nameof(baseAccess))]
    private ExpressionSyntax? BuildAccess(
        ExpressionSyntax? baseAccess,
        IEnumerable<MemberGetterPair> path,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false
    )
    {
        if (nullConditional)
        {
            return path.AggregateWithPrevious(
                baseAccess,
                (expr, prevProp, prop) => prop.Getter.BuildAccess(expr, prevProp.Member?.IsNullable == true)
            );
        }

        if (addValuePropertyOnNullable)
        {
            return path.Aggregate(
                baseAccess,
                (a, b) =>
                    b.Member.Type.IsNullableValueType()
                        ? MemberAccess(b.Getter.BuildAccess(a), NullableValueProperty)
                        : b.Getter.BuildAccess(a)
            );
        }

        return path.Aggregate(baseAccess, (a, b) => b.Getter.BuildAccess(a));
    }

    /// <summary>
    /// Builds a condition (the resulting expression evaluates to a boolean)
    /// whether the path is non-null.
    /// </summary>
    /// <param name="baseAccess">The base access to access the member or <c>null</c>.</param>
    /// <param name="useNullConditionalAccess">Whether null conditional member access can be used.</param>
    /// <returns><c>null</c> if no part of the path is nullable or the condition which needs to be true,
    /// that the path cannot be <c>null</c>.</returns>
    public ExpressionSyntax? BuildNotNullCondition(ExpressionSyntax baseAccess, bool useNullConditionalAccess)
    {
        return useNullConditionalAccess ? BuildNonNullCondition(baseAccess) : BuildNonNullConditionWithoutConditionalAccess(baseAccess);
    }

    private BinaryExpressionSyntax BuildNonNullCondition(ExpressionSyntax baseAccess)
    {
        return IsNotNull(BuildAccess(baseAccess, nullConditional: true, skipTrailingNonNullable: true));
    }

    private ExpressionSyntax? BuildNonNullConditionWithoutConditionalAccess(ExpressionSyntax baseAccess)
    {
        var nullablePath = PathWithoutTrailingNonNullable();
        var access = baseAccess;
        var conditions = new List<BinaryExpressionSyntax>();
        foreach (var pathPart in nullablePath)
        {
            access = pathPart.Getter.BuildAccess(access);

            if (!pathPart.Member.IsNullable)
                continue;

            conditions.Add(IsNotNull(access));
        }

        return conditions.Count == 0 ? null : And(conditions);
    }

    private IEnumerable<MemberGetterPair> PathWithoutTrailingNonNullable() =>
        _path.Reverse().SkipWhile(x => !x.Member.IsNullable).Reverse();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        var other = (MemberPathGetter)obj;
        return MemberPath.Equals(other.MemberPath);
    }

    public override int GetHashCode() => MemberPath.GetHashCode();
}
