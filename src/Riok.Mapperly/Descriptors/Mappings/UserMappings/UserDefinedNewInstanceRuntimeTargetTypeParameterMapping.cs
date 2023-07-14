using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A mapping which has a <see cref="Type"/> as a second parameter describing the target type of the mapping.
/// Generates a switch expression based on the mapping types.
/// </summary>
public class UserDefinedNewInstanceRuntimeTargetTypeParameterMapping : UserDefinedNewInstanceRuntimeTargetTypeMapping
{
    private readonly MethodParameter _targetTypeParameter;

    public UserDefinedNewInstanceRuntimeTargetTypeParameterMapping(
        IMethodSymbol method,
        RuntimeTargetTypeMappingMethodParameters parameters,
        bool enableReferenceHandling,
        NullFallbackValue nullArm,
        ITypeSymbol objectType
    )
        : base(method, parameters.Source, parameters.ReferenceHandler, enableReferenceHandling, nullArm, objectType)
    {
        _targetTypeParameter = parameters.TargetType;
    }

    protected override ParameterListSyntax BuildParameterList() =>
        ParameterList(IsExtensionMethod, SourceParameter, _targetTypeParameter, ReferenceHandlerParameter);

    protected override ExpressionSyntax BuildTargetType() => IdentifierName(_targetTypeParameter.Name);
}
