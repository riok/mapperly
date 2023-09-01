using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.ObjectFactories;
using Riok.Mapperly.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via an object factory.
/// </summary>
public class NewInstanceObjectFactoryMemberMapping : ObjectMemberMethodMapping
{
    private const string TargetVariableName = "target";
    private readonly ObjectFactory _objectFactory;
    private readonly bool _enableReferenceHandling;

    public NewInstanceObjectFactoryMemberMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ObjectFactory objectFactory,
        bool enableReferenceHandling
    )
        : base(sourceType, targetType)
    {
        _objectFactory = objectFactory;
        _enableReferenceHandling = enableReferenceHandling;
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        if (_enableReferenceHandling)
        {
            // TryGetReference
            yield return ReferenceHandlingSyntaxFactoryHelper.TryGetReference(ctx, this);
        }

        // var target = CreateMyObject<T>();
        yield return ctx.SyntaxFactory.DeclareLocalVariable(
            targetVariableName,
            _objectFactory.CreateType(SourceType, TargetType, ctx.Source)
        );

        // set the reference as soon as it is created,
        // as property mappings could refer to the same instance.
        if (_enableReferenceHandling)
        {
            // SetReference
            yield return ctx.SyntaxFactory.ExpressionStatement(
                ReferenceHandlingSyntaxFactoryHelper.SetReference(this, ctx, IdentifierName(targetVariableName))
            );
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
