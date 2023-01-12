using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings;

/// <summary>
/// Represents a mapping method declared but not implemented by the user which reuses an existing target object instance.
/// Is implicitly an <see cref="ObjectPropertyMapping"/>, since no other mappings work with an existing target object instance.
/// </summary>
public class UserDefinedExistingInstanceMethodMapping : ObjectPropertyMapping, IUserMapping
{
    private readonly bool _enableReferenceHandling;
    private readonly INamedTypeSymbol _referenceHandlerType;

    public UserDefinedExistingInstanceMethodMapping(
        IMethodSymbol method,
        MethodParameter sourceParameter,
        MethodParameter targetParameter,
        MethodParameter? referenceHandlerParameter,
        bool enableReferenceHandling,
        INamedTypeSymbol referenceHandlerType)
        : base(sourceParameter, targetParameter.Type)
    {
        _enableReferenceHandling = enableReferenceHandling;
        _referenceHandlerType = referenceHandlerType;
        IsPartial = true;
        IsExtensionMethod = method.IsExtensionMethod;
        Accessibility = method.DeclaredAccessibility;
        Method = method;
        MethodName = method.Name;
        TargetParameter = targetParameter;
        ReferenceHandlerParameter = referenceHandlerParameter;
    }

    public IMethodSymbol Method { get; }

    private MethodParameter TargetParameter { get; }

    public override bool CallableByOtherMappings => false;

    protected override ITypeSymbol? ReturnType => null; // return type is always void.

    public override ExpressionSyntax Build(TypeMappingBuildContext ctx)
        => throw new InvalidOperationException($"{nameof(UserDefinedExistingInstanceMethodMapping)} does not support {nameof(Build)}");

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // if the source type is nullable, add a null guard.
        if (SourceType.IsNullable())
        {
            yield return IfNullReturn(ctx.Source);
        }

        // if reference handling is enabled and no reference handler parameter is declared
        // a new reference handler is instantiated and used.
        if (_enableReferenceHandling && ReferenceHandlerParameter == null)
        {
            // var refHandler = new RefHandler();
            var referenceHandlerName = ctx.NameBuilder.New(DefaultReferenceHandlerParameterName);
            var createRefHandler = CreateInstance(_referenceHandlerType);
            yield return DeclareLocalVariable(referenceHandlerName, createRefHandler);
            ctx = ctx.WithRefHandler(referenceHandlerName);
        }

        foreach (var body in base.BuildBody(ctx, IdentifierName(TargetParameter.Name)))
        {
            yield return body;
        }
    }

    protected override ParameterListSyntax BuildParameterList()
        // needs to include the target parameter
        => ParameterList(IsExtensionMethod, SourceParameter, TargetParameter, ReferenceHandlerParameter);

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
    }
}
