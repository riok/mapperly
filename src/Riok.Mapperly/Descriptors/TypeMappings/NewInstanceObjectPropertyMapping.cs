using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

public class NewInstanceObjectPropertyMapping : ObjectPropertyMapping
{
    private const string TargetVariableName = "target";

    public NewInstanceObjectPropertyMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType)
        : base(sourceType, targetType)
    {
    }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        // if the source type is nullable, add a null guard.
        if (SourceType.IsNullable())
        {
            yield return IfNullReturn(source, NullSubstitute(TargetType, source));
        }

        // var target = new T();
        yield return CreateInstance(TargetVariableName, TargetType);

        // map properties
        foreach (var expression in base.BuildBody(source, IdentifierName(TargetVariableName)))
        {
            yield return expression;
        }

        // return target;
        yield return ReturnVariable(TargetVariableName);
    }
}
