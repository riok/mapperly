using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which results in a new target object instance.
/// </summary>
public class UserDefinedNewInstanceMethodMapping : MethodMapping, IUserMapping
{
    private const string NoMappingComment = "// Could not generate mapping";

    public UserDefinedNewInstanceMethodMapping(IMethodSymbol method)
        : base(method.Parameters.Single().Type.UpgradeNullable(), method.ReturnType.UpgradeNullable())
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

    public TypeMapping? DelegateMapping { get; set; }

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        if (DelegateMapping == null)
        {
            return new StatementSyntax[]
            {
                ThrowStatement(ThrowNotImplementedException())
                    .WithLeadingTrivia(TriviaList(Comment(NoMappingComment))),
            };
        }

        if (DelegateMapping is MethodMapping delegateMethodMapping)
        {
            return delegateMethodMapping.BuildBody(source);
        }

        return new[] { ReturnStatement(DelegateMapping.Build(source)) };
    }
}
