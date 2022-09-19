using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.ObjectFactories;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// An object mapping creating the target instance via an object factory.
/// </summary>
public class NewInstanceObjectFactoryPropertyMapping : ObjectPropertyMapping
{
    private const string TargetVariableName = "target";
    private readonly ObjectFactory _objectFactory;

    public NewInstanceObjectFactoryPropertyMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ObjectFactory objectFactory)
        : base(sourceType, targetType)
    {
        _objectFactory = objectFactory;
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // var target = CreateMyObject<T>();
        yield return DeclareLocalVariable(TargetVariableName, _objectFactory.CreateType(TargetType));

        // map properties
        foreach (var expression in BuildBody(source, IdentifierName(TargetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ReturnVariable(TargetVariableName);
    }
}
