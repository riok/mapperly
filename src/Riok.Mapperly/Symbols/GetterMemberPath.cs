using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

/// <summary>
/// Wraps a readable member.
/// Could be a directly accessible member
/// or one that is only accessible with an unsafe accessor method, <seealso cref="UnsafeAccessorContext"/>.
/// </summary>
[DebuggerDisplay("{MemberPath}")]
public class GetterMemberPath : IEquatable<GetterMemberPath>
{
    private const string NullableValueProperty = "Value";

    private GetterMemberPath(MemberPath memberPath)
    {
        MemberPath = memberPath;
    }

    public MemberPath MemberPath { get; }

    public static GetterMemberPath Build(MappingBuilderContext ctx, MemberPath memberPath)
    {
        if (memberPath.Path.Count == 0)
        {
            return new GetterMemberPath(memberPath);
        }

        var memberPathArray = memberPath.Path.Select(item => BuildMappableMember(ctx, item)).ToArray();
        return new GetterMemberPath(new NonEmptyMemberPath(memberPath.RootType, memberPathArray));
    }

    public static IEnumerable<IMappableMember> Build(MappingBuilderContext ctx, IEnumerable<IMappableMember> path)
    {
        return path.Select(item => BuildMappableMember(ctx, item));
    }

    private static IMappableMember BuildMappableMember(MappingBuilderContext ctx, IMappableMember item)
    {
        if (ctx.SymbolAccessor.IsDirectlyAccessible(item.MemberSymbol))
        {
            return item;
        }

        if (item.MemberSymbol.Kind == SymbolKind.Field)
        {
            var unsafeFieldAccessor = ctx.UnsafeAccessorContext.GetOrBuildAccessor(
                UnsafeAccessorContext.UnsafeAccessorType.GetField,
                (IFieldSymbol)item.MemberSymbol
            );

            return new MethodAccessorMember(item, unsafeFieldAccessor.MethodName);
        }

        var unsafeGetAccessor = ctx.UnsafeAccessorContext.GetOrBuildAccessor(
            UnsafeAccessorContext.UnsafeAccessorType.GetProperty,
            (IPropertySymbol)item.MemberSymbol
        );

        return new MethodAccessorMember(item, unsafeGetAccessor.MethodName);
    }

    public ExpressionSyntax BuildAccess(
        ExpressionSyntax baseAccess,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false,
        bool skipTrailingNonNullable = false
    )
    {
        var path = skipTrailingNonNullable ? MemberPath.PathWithoutTrailingNonNullable() : MemberPath.Path;

        if (nullConditional)
        {
            return path.AggregateWithPrevious(
                baseAccess,
                (expr, prevProp, prop) => prevProp?.IsNullable == true ? prop.BuildAccess(expr, true) : prop.BuildAccess(expr)
            );
        }

        if (addValuePropertyOnNullable)
        {
            return path.Aggregate(
                baseAccess,
                (a, b) =>
                    b.Type.IsNullableValueType()
                        ? SyntaxFactoryHelper.MemberAccess(b.BuildAccess(a), NullableValueProperty)
                        : b.BuildAccess(a)
            );
        }

        return path.Aggregate(baseAccess, (a, b) => b.BuildAccess(a));
    }

    public bool Equals(GetterMemberPath other) => MemberPath.Equals(other.MemberPath);

    bool IEquatable<GetterMemberPath>.Equals(GetterMemberPath? other) => other is not null && Equals(other);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is GetterMemberPath getterBuilder && Equals(getterBuilder);
    }

    public override int GetHashCode() => MemberPath.GetHashCode();

    public static bool operator ==(GetterMemberPath? left, GetterMemberPath? right) => Equals(left, right);

    public static bool operator !=(GetterMemberPath? left, GetterMemberPath? right) => !Equals(left, right);
}
