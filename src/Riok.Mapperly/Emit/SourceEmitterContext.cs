using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Emit;

public record SourceEmitterContext(bool IsStatic, UniqueNameBuilder NameBuilder, SyntaxFactoryHelper SyntaxFactory)
{
    public SourceEmitterContext AddIndentation() => this with { SyntaxFactory = SyntaxFactory.AddIndentation() };

    public SourceEmitterContext RemoveIndentation() => this with { SyntaxFactory = SyntaxFactory.RemoveIndentation() };
}
