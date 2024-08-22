using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a tuple expression (eg. (A: 10, B: 20)),
/// mapping properties via ctor, not by assigning.
/// <seealso cref="NewInstanceObjectMemberMethodMapping"/>
/// </summary>
public class NewValueTupleExpressionMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    int argumentCount,
    bool enableAggressiveInlining
) : ObjectMemberMethodMapping(sourceType, targetType, enableAggressiveInlining), INewValueTupleMapping
{
    private const string TargetVariableName = "target";
    private readonly HashSet<ValueTupleConstructorParameterMapping> _constructorPropertyMappings = new();

    public void AddConstructorParameterMapping(ValueTupleConstructorParameterMapping mapping) => _constructorPropertyMappings.Add(mapping);

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        // generate error if constructor argument don't match
        if (_constructorPropertyMappings.Count != argumentCount)
        {
            return ctx.SyntaxFactory.ThrowMappingNotImplementedException();
        }

        return base.Build(ctx);
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // generate error if constructor argument don't match
        if (_constructorPropertyMappings.Count != argumentCount)
        {
            yield return ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException());
            yield break;
        }

        // (Name:.. ,..);
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(ctx, emitFieldName: true));
        var tupleCreationExpression = TupleExpression(CommaSeparatedList(ctorArgs));

        // var target = (Name:.. ,..);
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);
        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, tupleCreationExpression);

        // map properties
        // target.Name.Child = ...
        foreach (var expression in BuildBody(ctx, IdentifierName(targetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ctx.SyntaxFactory.ReturnVariable(targetVariableName);
    }
}
