using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

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
public class EnsureCapacityNonEnumerated : EnsureCapacity
{
    private readonly string _targetAccessor;
    private readonly IMethodSymbol _getNonEnumeratedMethod;

    public EnsureCapacityNonEnumerated(string targetAccessor, IMethodSymbol getNonEnumeratedMethod)
    {
        _targetAccessor = targetAccessor;
        _getNonEnumeratedMethod = getNonEnumeratedMethod;
    }

    public override StatementSyntax Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var targetCount = MemberAccess(target, _targetAccessor);

        var countIdentifier = Identifier(ctx.NameBuilder.New("sourceCount"));
        var countIdentifierName = IdentifierName(countIdentifier);

        var enumerableArgument = Argument(ctx.Source);

        var outVarArgument = Argument(
            DeclarationExpression(
                VarIdentifier,
                SingleVariableDesignation(countIdentifier)))
            .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

        var getNonEnumeratedInvocation = StaticInvocation(_getNonEnumeratedMethod, enumerableArgument, outVarArgument);
        return IfStatement(getNonEnumeratedInvocation, Block(EnsureCapacityStatement(target, countIdentifierName, targetCount)));
    }
}
