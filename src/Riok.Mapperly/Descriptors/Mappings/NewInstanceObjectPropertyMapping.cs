using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings.PropertyMappings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

public class NewInstanceObjectPropertyMapping : ObjectPropertyMapping
{
    private const string TargetVariableName = "target";
    private readonly HashSet<ConstructorParameterMapping> _constructorPropertyMappings = new();
    private readonly HashSet<PropertyAssignmentMapping> _initPropertyMappings = new();

    public NewInstanceObjectPropertyMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
        => _constructorPropertyMappings.Add(mapping);

    public void AddInitPropertyMapping(PropertyAssignmentMapping mapping)
        => _initPropertyMappings.Add(mapping);

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // new T() { ... };
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(source)).ToArray();
        var objectCreationExpression = CreateInstance(TargetType, ctorArgs);

        // add initializer
        if (_initPropertyMappings.Count > 0)
        {
            var initMappings = _initPropertyMappings
                .Select(x => x.BuildExpression(source, null))
                .ToArray();
            objectCreationExpression = objectCreationExpression.WithInitializer(ObjectInitializer(initMappings));
        }

        // var target = new T() { ... };
        yield return DeclareLocalVariable(TargetVariableName, objectCreationExpression);

        // map properties
        foreach (var expression in BuildBody(source, IdentifierName(TargetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ReturnVariable(TargetVariableName);
    }
}
