using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;

namespace Riok.Mapperly.Descriptors.Constructors;

/// <summary>
/// An instance constructor represents code-to-be-generated
/// which creates a new object instance.
/// This could happen by calling a C# instance constructor,
/// an unsafe accessor or by calling an object factory.
/// </summary>
public interface IInstanceConstructor
{
    /// <summary>
    /// Whether this constructor supports object initialization blocks to initialize properties.
    /// </summary>
    bool SupportsObjectInitializer { get; }

    ExpressionSyntax CreateInstance(
        TypeMappingBuildContext ctx,
        IEnumerable<ArgumentSyntax> args,
        InitializerExpressionSyntax? initializer = null
    );
}
