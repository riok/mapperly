using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping from an array to an array of the same type by using Array.Clone.
/// </summary>
public class ArrayCloneMapping : TypeMapping
{
    private const string CloneMethodName = nameof(Array.Clone);

    public ArrayCloneMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
    }

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        return CastExpression(
           FullyQualifiedIdentifier(TargetType),
            InvocationExpression(MemberAccess(ctx.Source, CloneMethodName)));
    }
}
