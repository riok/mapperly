using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

/// <summary>
/// Represents a foreach enumerable mapping which works by looping through the source,
/// mapping each element and adding it to the target collection.
/// </summary>
public class ForEachAddEnumerableExistingTargetMapping : ExistingTargetMapping
{
    private const string LoopItemVariableName = "item";
    private const string AddMethodName = nameof(ICollection<object>.Add);

    private readonly ITypeMapping _elementMapping;

    public ForEachAddEnumerableExistingTargetMapping(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        ITypeMapping elementMapping)
        : base(sourceType, targetType)
    {
        _elementMapping = elementMapping;
    }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        var loopItemVariableName = ctx.NameBuilder.New(LoopItemVariableName);
        var convertedSourceItemExpression = _elementMapping.Build(ctx.WithSource(loopItemVariableName));
        var addMethod = MemberAccess(target, AddMethodName);

        return new StatementSyntax[]
        {
            ForEachStatement(
                VarIdentifier,
                Identifier(loopItemVariableName),
                ctx.Source,
                Block(ExpressionStatement(Invocation(addMethod, convertedSourceItemExpression))))
        };
    }
}
