using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class ReferenceHandlingSyntaxFactoryHelper
{
    private const string ExistingTargetVariableName = "existingTargetReference";

    public static IfStatementSyntax TryGetReference(
        ITypeMapping mapping,
        TypeMappingBuildContext ctx)
    {
        // GetReference<TSource, TTarget>
        var refHandler = ctx.ReferenceHandler ?? throw new ArgumentNullException(nameof(ctx.ReferenceHandler));
        var methodName = GenericName(Identifier(nameof(IReferenceHandler.TryGetReference)))
            .WithTypeArgumentList(TypeArgumentList(IdentifierName(mapping.SourceType.ToDisplayString()), IdentifierName(mapping.TargetType.ToDisplayString())));
        var method = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, refHandler, methodName);

        // out var target
        var targetArgument = Argument(
                DeclarationExpression(
                    VarIdentifier,
                    SingleVariableDesignation(Identifier(ExistingTargetVariableName))))
            .WithRefOrOutKeyword(Token(SyntaxKind.OutKeyword));

        // GetReference<TSource, TTarget>(source, out var target)
        var invocation = Invocation(method, Argument(ctx.Source), targetArgument);

        // if (_referenceHandler.GetReference<TSource, TTarget>(source, out var target))
        //   return target;
        return IfStatement(
            invocation,
            ReturnStatement(IdentifierName(ExistingTargetVariableName)));
    }

    public static ExpressionSyntax SetReference(
        ITypeMapping mapping,
        TypeMappingBuildContext ctx,
        ExpressionSyntax target)
    {
        // SetReference<TSource, TTarget>
        var refHandler = ctx.ReferenceHandler ?? throw new ArgumentNullException(nameof(ctx.ReferenceHandler));
        var methodName = GenericName(Identifier(nameof(IReferenceHandler.SetReference)))
            .WithTypeArgumentList(TypeArgumentList(IdentifierName(mapping.SourceType.ToDisplayString()), IdentifierName(mapping.TargetType.ToDisplayString())));
        var method = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, refHandler, methodName);

        return Invocation(method, ctx.Source, target);
    }
}
