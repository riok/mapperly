using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Symbols;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which results in a new target object instance.
/// </summary>
public class UserDefinedNewInstanceMethodMapping(
    IMethodSymbol method,
    bool? isDefault,
    MethodParameter sourceParameter,
    MethodParameter? referenceHandlerParameter,
    ITypeSymbol targetType,
    bool enableReferenceHandling
) : NewInstanceMethodMapping(method, sourceParameter, referenceHandlerParameter, targetType), INewInstanceUserMapping
{
    public new IMethodSymbol Method { get; } = method;

    public bool? Default { get; } = isDefault;

    public bool IsExternal => false;

    public INewInstanceMapping? DelegateMapping { get; private set; }

    /// <summary>
    /// The reference handling is enabled but is only internal to this method.
    /// No reference handler parameter is passed.
    /// </summary>
    public bool InternalReferenceHandlingEnabled => enableReferenceHandling && ReferenceHandlerParameter == null;

    public void SetDelegateMapping(INewInstanceMapping mapping) => DelegateMapping = mapping;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        return InternalReferenceHandlingEnabled ? DelegateMapping?.Build(ctx) ?? base.Build(ctx) : base.Build(ctx);
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (DelegateMapping == null)
        {
            return new[] { ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedExceptionStatement()) };
        }

        // the generated mapping method is called with a new reference handler instance
        // otherwise the generated method is embedded
        if (InternalReferenceHandlingEnabled)
        {
            // new RefHandler();
            var createRefHandler = ctx.SyntaxFactory.CreateInstance<PreserveReferenceHandler>();
            ctx = ctx.WithRefHandler(createRefHandler);
            return new[] { ctx.SyntaxFactory.Return(DelegateMapping.Build(ctx)) };
        }

        if (DelegateMapping is MethodMapping delegateMethodMapping)
            return delegateMethodMapping.BuildBody(ctx);

        return new[] { ctx.SyntaxFactory.Return(DelegateMapping.Build(ctx)) };
    }

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
        // if the user did not define a parameter a new reference handler is initialized
        if (DelegateMapping is MethodMapping methodMapping)
        {
            methodMapping.EnableReferenceHandling(iReferenceHandlerType);
        }
    }
}
