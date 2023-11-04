using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Symbols;

public class SetterMemberPath : MemberPath
{
    private SetterMemberPath(IReadOnlyList<IMappableMember> path, bool isMethod)
        : base(path)
    {
        IsMethod = isMethod;
    }

    /// <summary>
    /// Indicates whether this setter is an UnsafeAccessor for a property, ie. target.SetValue(source.Value);
    /// False for standard properties, fields and UnsafeAccessor fields.
    /// </summary>
    public bool IsMethod { get; }

    public static SetterMemberPath Build(MappingBuilderContext ctx, MemberPath memberPath)
    {
        // object path is the same as a getter
        var setterPath = GetterMemberPath.Build(ctx, memberPath.ObjectPath).ToList();
        // build the final member in the path and add it to the setter path
        var (member, isMethod) = BuildMemberSetter(ctx, memberPath.Member);
        setterPath.Add(member);

        return new SetterMemberPath(setterPath, isMethod);
    }

    private static (IMappableMember, bool) BuildMemberSetter(MappingBuilderContext ctx, IMappableMember member)
    {
        if (ctx.SymbolAccessor.IsDirectlyAccessible(member.MemberSymbol))
        {
            return (member, false);
        }

        if (member.MemberSymbol.Kind == SymbolKind.Field)
        {
            var unsafeFieldAccessor = ctx.UnsafeAccessorContext.GetOrBuildAccessor(
                UnsafeAccessorContext.UnsafeAccessorType.GetField,
                (IFieldSymbol)member.MemberSymbol
            );

            return (new MethodAccessorMember(member, unsafeFieldAccessor.MethodName), false);
        }

        var unsafeGetAccessor = ctx.UnsafeAccessorContext.GetOrBuildAccessor(
            UnsafeAccessorContext.UnsafeAccessorType.SetProperty,
            (IPropertySymbol)member.MemberSymbol
        );

        return (new MethodAccessorMember(member, unsafeGetAccessor.MethodName, methodRequiresParameter: true), true);
    }

    public ExpressionSyntax BuildAssignment(ExpressionSyntax? baseAccess, ExpressionSyntax sourceValue, bool coalesceAssignment = false)
    {
        IEnumerable<IMappableMember> path = Path;

        if (baseAccess == null)
        {
            baseAccess = SyntaxFactory.IdentifierName(Path[0].Name);
            path = path.Skip(1);
        }

        var memberPath = path.Aggregate(baseAccess, (a, b) => b.BuildAccess(a));

        if (coalesceAssignment)
        {
            // cannot use coalesce assignment within a setter method invocation.
            Debug.Assert(!IsMethod);

            // target.Value ??= mappedValue;
            return CoalesceAssignment(memberPath, sourceValue);
        }

        if (IsMethod)
        {
            // target.SetValue(source.Value);
            return Invocation(memberPath, sourceValue);
        }

        // target.Value = source.Value;
        return Assignment(memberPath, sourceValue);
    }
}
