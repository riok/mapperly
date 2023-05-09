using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which results in a new target object instance.
/// </summary>
public class UserDefinedNewInstanceMethodMapping : MethodMapping, IUserMapping
{
    private const string NoMappingComment = "// Could not generate mapping";

    private readonly bool _enableReferenceHandling;
    private readonly INamedTypeSymbol _referenceHandlerType;

    private ITypeMapping? _delegateMapping;

    public UserDefinedNewInstanceMethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter? referenceHandlerParameter,
        bool enableReferenceHandling,
        INamedTypeSymbol referenceHandlerType
    )
        : base(sourceParameter, method.ReturnType.UpgradeNullable())
    {
        _enableReferenceHandling = enableReferenceHandling;
        _referenceHandlerType = referenceHandlerType;
        IsPartial = true;
        IsExtensionMethod = method.IsExtensionMethod;
        Accessibility = method.DeclaredAccessibility;
        Method = method;
        MethodName = method.Name;
        ReferenceHandlerParameter = referenceHandlerParameter;
    }

    public IMethodSymbol Method { get; }

    public void SetDelegateMapping(ITypeMapping delegateMapping) => _delegateMapping = delegateMapping;

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping == null)
        {
            return new[] { ExpressionStatement(ThrowNotImplementedException()).WithLeadingTrivia(TriviaList(Comment(NoMappingComment))), };
        }

        // if reference handling is enabled and no reference handler parameter is declared
        // the generated mapping method is called with a new reference handler instance
        // otherwise the generated method is embedded
        if (_enableReferenceHandling && ReferenceHandlerParameter == null)
        {
            // new RefHandler();
            var createRefHandler = CreateInstance(_referenceHandlerType);
            ctx = ctx.WithRefHandler(createRefHandler);
            return new[] { ReturnStatement(_delegateMapping.Build(ctx)) };
        }

        if (_delegateMapping is MethodMapping delegateMethodMapping)
            return delegateMethodMapping.BuildBody(ctx);

        return new[] { ReturnStatement(_delegateMapping.Build(ctx)) };
    }

    /// <summary>
    /// A <see cref="UserDefinedNewInstanceMethodMapping"/> is callable by other mappings
    /// if either reference handling is not activated, or the user defined a reference handler parameter.
    /// </summary>
    public override bool CallableByOtherMappings => !_enableReferenceHandling || ReferenceHandlerParameter != null;

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
        // if the user did not define a parameter a new reference handler is initialized
        if (_delegateMapping is MethodMapping methodMapping)
        {
            methodMapping.EnableReferenceHandling(iReferenceHandlerType);
        }
    }
}
