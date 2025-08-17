using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Wraps an existing-target mapping and first tries to delegate mapping to a DI-provided <see cref="IExistingMapper{TSource,TDestination}"/>
/// if absent, uses the inner mapping. Only used for instance mappers and statements (not expressions).
/// </summary>
public class DiFirstExistingTargetDelegateMapping(
    IExistingTargetMapping innerMapping,
    string cacheFieldName,
    ITypeSymbol sourceType,
    ITypeSymbol targetType
) : ExistingTargetMapping(sourceType, targetType)
{
    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // Build:
        // var __di = this.GetOrNull_<cache>();
        // if (__di != null) { __di.Map(source, target); } else { <inner> }
        var thisExpr = ThisExpression();
        var helperName = "GetOrNull_" + cacheFieldName.TrimStart('_');
        var helperCall = ctx.SyntaxFactory.Invocation(MemberAccess(thisExpr, helperName));

        var diVarName = ctx.NameBuilder.New("diMapper");
        var diVarId = IdentifierName(diVarName);
        var decl = ctx.SyntaxFactory.DeclareLocalVariable(diVarName, helperCall);

        var notNull = IsNotNull(diVarId);
        var mapCall = ctx.SyntaxFactory.Invocation(MemberAccess(diVarId, "Map"), ctx.Source, target);
        var thenStmt = ctx.SyntaxFactory.AddIndentation().ExpressionStatement(mapCall);
        var elseBody = innerMapping.Build(ctx, target).Select(s => s.AddIndentation()).ToArray();
        var ifStmt = ctx.SyntaxFactory.If(notNull, [thenStmt], elseBody.Length == 0 ? null : elseBody);
        return [decl, ifStmt];
    }
}
