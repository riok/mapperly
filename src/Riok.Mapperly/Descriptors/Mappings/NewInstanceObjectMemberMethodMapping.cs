using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.MemberMappings;
using Riok.Mapperly.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call,
/// mapping properties via ctor, object initializer and by assigning.
/// </summary>
public class NewInstanceObjectMemberMethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType, bool enableReferenceHandling)
    : ObjectMemberMethodMapping(sourceType, targetType),
        INewInstanceObjectMemberMapping
{
    private const string TargetVariableName = "target";
    private readonly HashSet<ConstructorParameterMapping> _constructorPropertyMappings = new();
    private readonly HashSet<MemberAssignmentMapping> _initPropertyMappings = new();

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping) => _constructorPropertyMappings.Add(mapping);

    public void AddInitMemberMapping(MemberAssignmentMapping mapping) => _initPropertyMappings.Add(mapping);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        if (enableReferenceHandling)
        {
            // TryGetReference
            yield return ReferenceHandlingSyntaxFactoryHelper.TryGetReference(ctx, this);
        }

        // new T(ctorArgs) { ... };
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(ctx)).ToArray();
        var objectCreationExpression = CreateInstance(TargetType, ctorArgs);

        // add initializer
        if (_initPropertyMappings.Count > 0)
        {
            var initPropertiesContext = ctx.AddIndentation();
            var initMappings = _initPropertyMappings.Select(x => x.BuildExpression(initPropertiesContext, null)).ToArray();
            objectCreationExpression = objectCreationExpression.WithInitializer(ctx.SyntaxFactory.ObjectInitializer(initMappings));
        }

        // var target = new T() { ... };
        yield return ctx.SyntaxFactory.DeclareLocalVariable(targetVariableName, objectCreationExpression);

        // set the reference as soon as it is created,
        // as property mappings could refer to the same instance.
        if (enableReferenceHandling)
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
