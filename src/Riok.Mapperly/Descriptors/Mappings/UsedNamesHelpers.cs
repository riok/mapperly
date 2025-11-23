namespace Riok.Mapperly.Descriptors.Mappings;

public static class UsedNamesHelpers
{
    public static IEnumerable<string> ExtractUsedName<T>(T anyObject)
        where T : notnull => anyObject is IHasUsedNames objectWithParameters ? objectWithParameters.ExtractUsedParameters() : [];

    public static IEnumerable<string> ExtractUsedNames<T>(IEnumerable<T> collection)
        where T : notnull => collection.SelectMany(ExtractUsedName);
}
