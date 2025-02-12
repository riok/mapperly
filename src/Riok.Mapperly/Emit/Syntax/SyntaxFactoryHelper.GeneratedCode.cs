using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    private const string GeneratedCodeAttributeName = "global::System.CodeDom.Compiler.GeneratedCode";
    private static readonly AssemblyName _generatorAssemblyName = typeof(SyntaxFactoryHelper).Assembly.GetName();

    public AttributeListSyntax GeneratedCodeAttribute()
    {
        return Attribute(
            GeneratedCodeAttributeName,
            StringLiteral(_generatorAssemblyName.Name),
            StringLiteral(_generatorAssemblyName.Version.ToString())
        );
    }
}
