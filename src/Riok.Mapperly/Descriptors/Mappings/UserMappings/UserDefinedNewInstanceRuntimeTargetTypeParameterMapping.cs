using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A mapping which has a <see cref="Type"/> as a second parameter describing the target type of the mapping.
/// Generates a switch expression based on the mapping types.
/// </summary>
public class UserDefinedNewInstanceRuntimeTargetTypeParameterMapping(
    IMethodSymbol method,
    RuntimeTargetTypeMappingMethodParameters parameters,
    bool enableReferenceHandling,
    NullFallbackValue nullArm,
    ITypeSymbol objectType
)
    : UserDefinedNewInstanceRuntimeTargetTypeMapping(
        method,
        parameters.Source,
        parameters.ReferenceHandler,
        enableReferenceHandling,
        nullArm,
        objectType
    )
{
    protected override ParameterListSyntax BuildParameterList() =>
        ParameterList(IsExtensionMethod, SourceParameter, parameters.TargetType, ReferenceHandlerParameter);

    protected override ExpressionSyntax BuildTargetType() => IdentifierName(parameters.TargetType.Name);
}
