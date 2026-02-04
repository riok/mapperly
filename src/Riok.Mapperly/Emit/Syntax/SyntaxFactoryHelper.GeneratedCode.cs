using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Configuration;

namespace Riok.Mapperly.Emit.Syntax;

public partial struct SyntaxFactoryHelper
{
    public AttributeListSyntax GeneratedCodeAttribute()
    {
        return Attribute(
            MapperlyGeneratedCodeAttribute.GeneratedCodeAttributeName,
            StringLiteral(MapperlyGeneratedCodeAttribute.GeneratorToolName),
            StringLiteral(MapperlyGeneratedCodeAttribute.GeneratorToolVersion)
        );
    }
}
