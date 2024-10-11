using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Symbols.Members;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.Capacity;

/// <summary>
/// Represents a call to EnsureCapacity on a collection where there is an attempt
/// to get the number of elements in the source collection without enumeration,
/// calling EnsureCapacity if it is available.
/// </summary>
/// <remarks>
/// <code>
/// if(Enumerable.TryGetNonEnumeratedCount(source, out var sourceCount)
///     target.EnsureCapacity(sourceCount + target.Count);
/// </code>
/// </remarks>
public class NonEnumeratedCapacitySetter(
    ICapacityMemberSetter capacitySetter,
    IMemberGetter? targetAccessor,
    IMethodSymbol getNonEnumeratedMethod
) : ICapacitySetter
{
    private const string SourceCountVariableName = "sourceCount";

    public IMappableMember? CapacityTargetMember => capacitySetter.TargetCapacity;

    public StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var sourceCountName = ctx.NameBuilder.New(SourceCountVariableName);
        ExpressionSyntax count = IdentifierName(sourceCountName);
        if (targetAccessor != null)
        {
            count = Add(count, targetAccessor.BuildAccess(target));
        }

        var enumerableArgument = Argument(ctx.Source);
        var outVarArgument = OutVarArgument(sourceCountName);
        var getNonEnumeratedInvocation = ctx.SyntaxFactory.StaticInvocation(getNonEnumeratedMethod, enumerableArgument, outVarArgument);
        var setCapacity = ctx.SyntaxFactory.AddIndentation().ExpressionStatement(capacitySetter.BuildAssignment(target, count));
        return ctx.SyntaxFactory.If(getNonEnumeratedInvocation, setCapacity);
    }
}
