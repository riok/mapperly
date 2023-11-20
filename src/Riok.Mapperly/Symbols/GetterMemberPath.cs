using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Symbols;

public class GetterMemberPath : MemberPath
{
    private GetterMemberPath(IReadOnlyList<IMappableMember> path)
        : base(path) { }

    public static GetterMemberPath Build(MappingBuilderContext ctx, MemberPath memberPath)
    {
        var memberPathArray = memberPath.Path.Select(item => BuildMappableMember(ctx, item)).ToArray();
        return new GetterMemberPath(memberPathArray);
    }

    public static bool TryBuild(
        TypeMappingBuildContext ctx,
        MemberPath memberPath,
        [NotNullWhen(true)] out GetterMemberPath? getterMemberPath
    )
    {
        if (
            ctx.TrimSourcePath.Count > 0
            && memberPath.Path.Count >= ctx.TrimSourcePath.Count
            && memberPath.Path.Take(ctx.TrimSourcePath.Count).SequenceEqual(ctx.TrimSourcePath)
        )
        {
            getterMemberPath = new GetterMemberPath(memberPath.Path.Skip(ctx.TrimSourcePath.Count).ToArray());
            return true;
        }

        getterMemberPath = null;
        return false;
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
        ExpressionSyntax? baseAccess,
        bool addValuePropertyOnNullable = false,
        bool nullConditional = false,
        bool skipTrailingNonNullable = false
    )
    {
        var path = skipTrailingNonNullable ? PathWithoutTrailingNonNullable() : Path;

        if (baseAccess == null)
        {
            baseAccess = SyntaxFactory.IdentifierName(path.First().Name);
            path = path.Skip(1);
        }

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
}
