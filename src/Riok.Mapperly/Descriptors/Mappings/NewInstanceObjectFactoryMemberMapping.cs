using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via an object factory.
/// </summary>
public class NewInstanceObjectFactoryMemberMapping(
    ITypeSymbol sourceType,
    ITypeSymbol targetType,
    ObjectFactory objectFactory,
    bool enableReferenceHandling
) : ObjectMemberMethodMapping(sourceType, targetType)
{
    private const string TargetVariableName = "target";

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        // create instance
        foreach (var statement in objectFactory.CreateInstance(ctx, this, enableReferenceHandling, targetVariableName))
        {
            yield return statement;
        }

        // map properties
        foreach (var expression in BuildBody(ctx, IdentifierName(targetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ctx.SyntaxFactory.ReturnVariable(targetVariableName);
    }
}
