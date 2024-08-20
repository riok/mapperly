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
    private INewInstanceMapping? _delegateMapping;

    public new IMethodSymbol Method { get; } = method;

    public bool? Default { get; } = isDefault;

    public bool IsExternal => false;

    /// <summary>
    /// The reference handling is enabled but is only internal to this method.
    /// No reference handler parameter is passed.
    /// </summary>
    public bool InternalReferenceHandlingEnabled => enableReferenceHandling && ReferenceHandlerParameter == null;

    public void SetDelegateMapping(INewInstanceMapping mapping) => _delegateMapping = mapping;

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
    {
        return InternalReferenceHandlingEnabled ? _delegateMapping?.Build(ctx) ?? base.Build(ctx) : base.Build(ctx);
    }

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        if (_delegateMapping == null)
        {
            return [ctx.SyntaxFactory.ExpressionStatement(ctx.SyntaxFactory.ThrowMappingNotImplementedException())];
        }

        if (InternalReferenceHandlingEnabled)
        {
            // new RefHandler();
            var createRefHandler = ctx.SyntaxFactory.CreateInstance<PreserveReferenceHandler>();

            // If additional parameters are used or it is explicitly set as non-default, the method is embedded
            // as it cannot be reused by other mappings anyway (additional parameter mappings are never reused).
            if ((Default == false || AdditionalSourceParameters.Count > 0) && _delegateMapping is MethodMapping delMethodMapping)
            {
                var refHandlerName = ctx.NameBuilder.New(DefaultReferenceHandlerParameterName);

                // var refHandler = new RefHandler();
                var declareRefHandler = ctx.SyntaxFactory.DeclareLocalVariable(refHandlerName, createRefHandler);
                ctx = ctx.WithRefHandler(refHandlerName);
                return delMethodMapping.BuildBody(ctx).Prepend(declareRefHandler);
            }

            // the generated mapping method is called with a new reference handler instance
            ctx = ctx.WithRefHandler(createRefHandler);
            return [ctx.SyntaxFactory.Return(_delegateMapping.Build(ctx))];
        }

        if (_delegateMapping is MethodMapping delegateMethodMapping)
            return delegateMethodMapping.BuildBody(ctx);

        return [ctx.SyntaxFactory.Return(_delegateMapping.Build(ctx))];
    }

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
