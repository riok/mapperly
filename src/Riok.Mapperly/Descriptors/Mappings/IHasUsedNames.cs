namespace Riok.Mapperly.Descriptors.Mappings;

public interface IHasUsedNames
{
    IEnumerable<string> ExtractUsedParameters();
}
