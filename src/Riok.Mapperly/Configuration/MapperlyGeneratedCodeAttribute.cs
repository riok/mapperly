using System.Reflection;
using Riok.Mapperly.Emit.Syntax;

namespace Riok.Mapperly.Configuration;

internal static class MapperlyGeneratedCodeAttribute
{
    public const string GeneratedCodeAttributeName = "global::System.CodeDom.Compiler.GeneratedCode";

    private static readonly AssemblyName _generatorAssemblyName = typeof(SyntaxFactoryHelper).Assembly.GetName();

    public static readonly string GeneratorToolName = _generatorAssemblyName.Name;
    public static readonly string GeneratorToolVersion = _generatorAssemblyName.Version.ToString();
}
