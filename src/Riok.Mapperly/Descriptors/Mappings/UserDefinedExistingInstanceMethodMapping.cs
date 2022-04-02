using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which reuses an existing target object instance.
/// Is implicitly an <see cref="ObjectPropertyMapping"/>, since no other mappings work with an existing target object instance.
/// </summary>
public class UserDefinedExistingInstanceMethodMapping : ObjectPropertyMapping, IUserMapping
{
    public UserDefinedExistingInstanceMethodMapping(
        IMethodSymbol method)
        : base(method.Parameters[0].Type.UpgradeNullable(), method.Parameters[1].Type.UpgradeNullable())
    {
        Partial = true;
        IsStatic = method.IsStatic;
        IsExtensionMethod = method.IsExtensionMethod;
        Accessibility = method.DeclaredAccessibility;
        MappingSourceParameterName = method.Parameters[0].Name;
        Method = method;
        MethodName = method.Name;
    }

    public IMethodSymbol Method { get; }

    private IParameterSymbol TargetParameter => Method.Parameters[1];

    public override bool CallableByOtherMappings => false;

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => throw new InvalidOperationException($"{nameof(UserDefinedExistingInstanceMethodMapping)} does not support {nameof(Build)}");

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        var body = base.BuildBody(source, IdentifierName(TargetParameter.Name));

        // if the source type is nullable, add a null guard.
        return SourceType.IsNullable()
            ? body.Prepend(IfNullReturn(source))
            : body;
    }

    protected override ITypeSymbol? ReturnType => null; // return type is always void.

    protected override IEnumerable<ParameterSyntax> BuildParameters()
    {
        var targetParam = Parameter(Identifier(TargetParameter.Name))
            .WithType(IdentifierName(TargetType.ToDisplayString()));

        return base.BuildParameters().Append(targetParam);
    }
}
