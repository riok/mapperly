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

    public NewInstanceObjectPropertyMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
    }

    public void AddConstructorParameterMapping(ConstructorParameterMapping mapping)
        => _constructorPropertyMappings.Add(mapping);

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // var target = new T();
        var ctorArgs = _constructorPropertyMappings.Select(x => x.BuildArgument(source)).ToArray();
        yield return CreateInstance(TargetVariableName, TargetType, ctorArgs);

        // map properties
        foreach (var expression in BuildBody(source, IdentifierName(TargetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ReturnVariable(TargetVariableName);
    }
}
