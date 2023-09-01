using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Descriptors.Mappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class ReferenceHandlingSyntaxFactoryHelper
{
    private const string ExistingTargetVariableName = "existingTargetReference";

    public static IfStatementSyntax TryGetReference(TypeMappingBuildContext ctx, INewInstanceMapping mapping)
    {
        // GetReference<TSource, TTarget>
        var refHandler = ctx.ReferenceHandler ?? throw new ArgumentException("Reference handler is not set", nameof(ctx));
        var methodName = GenericName(Identifier(nameof(IReferenceHandler.TryGetReference)))
            .WithTypeArgumentList(
                TypeArgumentList(FullyQualifiedIdentifier(mapping.SourceType), FullyQualifiedIdentifier(mapping.TargetType))
            );
        var method = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, refHandler, methodName);

        // out var target
        var existingTargetVariableName = ctx.NameBuilder.New(ExistingTargetVariableName);
        var targetArgument = Argument(
                DeclarationExpression(VarIdentifier, SingleVariableDesignation(Identifier(existingTargetVariableName)))
            )
            .WithRefOrOutKeyword(TrailingSpacedToken(SyntaxKind.OutKeyword));

        // GetReference<TSource, TTarget>(source, out var target)
        var invocation = Invocation(method, Argument(ctx.Source), targetArgument);

        // if (_referenceHandler.GetReference<TSource, TTarget>(source, out var target))
        //   return target;
        return ctx.SyntaxFactory.If(invocation, ctx.SyntaxFactory.AddIndentation().Return(IdentifierName(existingTargetVariableName)));
    }

    public static ExpressionSyntax SetReference(INewInstanceMapping mapping, TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        // SetReference<TSource, TTarget>
        var refHandler = ctx.ReferenceHandler ?? throw new ArgumentException("Reference handler is not set", nameof(ctx));
        var methodName = GenericName(Identifier(nameof(IReferenceHandler.SetReference)))
            .WithTypeArgumentList(
                TypeArgumentList(FullyQualifiedIdentifier(mapping.SourceType), (FullyQualifiedIdentifier(mapping.TargetType)))
            );
        var method = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, refHandler, methodName);

        return Invocation(method, ctx.Source, target);
    }
}
