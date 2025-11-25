namespace Riok.Mapperly.Descriptors.Mappings;

public interface IHasUsedParameters
{
    IEnumerable<string> ExtractUsedParameters();
}
