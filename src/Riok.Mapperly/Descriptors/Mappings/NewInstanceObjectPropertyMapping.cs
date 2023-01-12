using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using Riok.Mapperly.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via a new() call.
/// </summary>
public class NewInstanceObjectPropertyMapping : ObjectPropertyMapping
{
    private const string TargetVariableName = "target";
    private readonly HashSet<ConstructorParameterMapping> _constructorPropertyMappings = new();
    private readonly HashSet<PropertyAssignmentMapping> _initPropertyMappings = new();
    private readonly bool _enableReferenceHandling;

    public NewInstanceObjectPropertyMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        bool enableReferenceHandling)
        : base(sourceType, targetType)
    {
        _enableReferenceHandling = enableReferenceHandling;
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
        => _constructorPropertyMappings.Add(mapping);

    public void AddInitPropertyMapping(PropertyAssignmentMapping mapping)
        => _initPropertyMappings.Add(mapping);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        var targetVariableName = ctx.NameBuilder.New(TargetVariableName);

        if (_enableReferenceHandling)
        {
            // TryGetReference
            yield return ReferenceHandlingSyntaxFactoryHelper.TryGetReference(this, ctx);
        }

        // new T(ctorArgs) { ... };
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(ctx)).ToArray();
        var objectCreationExpression = CreateInstance(TargetType, ctorArgs);

        // add initializer
        if (_initPropertyMappings.Count > 0)
        {
            var initMappings = _initPropertyMappings
                .Select(x => x.BuildExpression(ctx, null))
                .ToArray();
            objectCreationExpression = objectCreationExpression.WithInitializer(ObjectInitializer(initMappings));
        }

        // var target = new T() { ... };
        yield return DeclareLocalVariable(targetVariableName, objectCreationExpression);

        // set the reference as soon as it is created,
        // as property mappings could refer to the same instance.
        if (_enableReferenceHandling)
        {
            // SetReference
            yield return ExpressionStatement(ReferenceHandlingSyntaxFactoryHelper.SetReference(
                this,
                ctx,
                IdentifierName(targetVariableName)));
        }

        // map properties
        foreach (var expression in BuildBody(ctx, IdentifierName(targetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ReturnVariable(targetVariableName);
    }
}
