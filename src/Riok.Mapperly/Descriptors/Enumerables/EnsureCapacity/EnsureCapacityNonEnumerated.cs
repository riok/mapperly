using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

/// <summary>
/// Represents a call to EnsureCapacity on a collection where there is an attempt
/// to get the  number of elements in the source collection without enumeration,
/// calling EnsureCapacity if it is available.
/// </summary>
/// <remarks>
/// <code>
/// if(Enumerable.TryGetNonEnumeratedCount(source, out var sourceCount)
///     target.EnsureCapacity(sourceCount + target.Count);
/// </code>
/// </remarks>
public class EnsureCapacityNonEnumerated(IMemberGetter? targetAccessor, IMethodSymbol getNonEnumeratedMethod) : EnsureCapacityInfo
{
    private const string SourceCountVariableName = "sourceCount";

    public override StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var targetCount = targetAccessor?.BuildAccess(target);

        var sourceCountName = ctx.NameBuilder.New(SourceCountVariableName);

        var enumerableArgument = Argument(ctx.Source);
        var outVarArgument = OutVarArgument(sourceCountName);

        var getNonEnumeratedInvocation = StaticInvocation(getNonEnumeratedMethod, enumerableArgument, outVarArgument);
        var ensureCapacity = EnsureCapacityStatement(
            ctx.SyntaxFactory.AddIndentation(),
            target,
            IdentifierName(sourceCountName),
            targetCount
        );
        return ctx.SyntaxFactory.If(getNonEnumeratedInvocation, ensureCapacity);
    }
}
