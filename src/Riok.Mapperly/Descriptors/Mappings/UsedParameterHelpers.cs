namespace Riok.Mapperly.Descriptors.Mappings;

public static class UsedParameterHelpers
{
    public static IEnumerable<string> ExtractUsedParameters<T>(T anyObject)
        where T : notnull => anyObject is IHasUsedParameters objectWithParameters ? objectWithParameters.ExtractUsedParameters() : [];

    public static IEnumerable<string> ExtractUsedAllParameters<T>(IEnumerable<T> collection)
        where T : notnull => collection.SelectMany(ExtractUsedParameters);
}
