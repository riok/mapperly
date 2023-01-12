using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Emit;

public class SourceEmitterContext
{
    public SourceEmitterContext(bool isStatic, UniqueNameBuilder nameBuilder)
    {
        IsStatic = isStatic;
        NameBuilder = nameBuilder;
    }

    public bool IsStatic { get; }

    public UniqueNameBuilder NameBuilder { get; }
}
