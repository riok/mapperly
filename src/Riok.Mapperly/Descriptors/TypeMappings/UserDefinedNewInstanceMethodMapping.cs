using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which results in a new target object instance.
/// </summary>
public class UserDefinedNewInstanceMethodMapping : MethodMapping, IHasUserSymbolMapping
{
    private const string NoMappingComment = "// Could not generate mapping";

    public UserDefinedNewInstanceMethodMapping(IMethodSymbol method, bool isAbstractMapperDefinition)
        : base(method.Parameters.Single().Type, method.ReturnType)
    {
        Override = isAbstractMapperDefinition;
        Accessibility = Accessibility.Public;
        Method = method;
    }

    public IMethodSymbol Method { get; }

    public TypeMapping? DelegateMapping { get; set; }

    protected override string MethodName => Method.Name;

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
