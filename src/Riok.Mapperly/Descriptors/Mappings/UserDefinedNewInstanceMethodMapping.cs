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
    private TypeMapping? _delegateMapping;

    public UserDefinedNewInstanceMethodMapping(IMethodSymbol method)
        : base(method.Parameters.Single().Type.UpgradeNullable(), method.ReturnType.UpgradeNullable())
    {
        IsPartial = true;
        IsExtensionMethod = method.IsExtensionMethod;
        Accessibility = method.DeclaredAccessibility;
        MappingSourceParameterName = method.Parameters[0].Name;
        Method = method;
        MethodName = method.Name;
    }

    public IMethodSymbol Method { get; }

    public void SetDelegateMapping(TypeMapping delegateMapping)
        => _delegateMapping = delegateMapping;

    public override IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source)
    {
        if (_delegateMapping == null)
        {
            return new StatementSyntax[]
            {
                ThrowStatement(ThrowNotImplementedException())
                    .WithLeadingTrivia(TriviaList(Comment(NoMappingComment))),
            };
        }

        if (_delegateMapping is MethodMapping delegateMethodMapping)
        {
            return delegateMethodMapping.BuildBody(source);
        }

        return new[] { ReturnStatement(_delegateMapping.Build(source)) };
    }
}
