namespace Riok.Mapperly.Emit;

public class SourceEmitterContext
{
    public SourceEmitterContext(bool isStatic)
    {
        IsStatic = isStatic;
    }

    public bool IsStatic { get; }
}
