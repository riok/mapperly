using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Helpers;

public static class EnumNamingStrategyHelper
{
    public static IReadOnlyDictionary<IFieldSymbol, string> BuildCustomNameStrategyMappings(
        this MappingBuilderContext ctx,
        ITypeSymbol enumSymbol
    )
    {
        var customNameMappings = new Dictionary<IFieldSymbol, string>(SymbolEqualityComparer.Default);

        var values = ctx.SymbolAccessor.GetAllFields(enumSymbol);
        foreach (var value in values)
        {
            var valueString = ConvertEnumValueNameToString(value.Name, ctx.Configuration.Enum.NamingStrategy);
            customNameMappings.Add(value, valueString);
        }

        return customNameMappings;
    }

    private static string ConvertEnumValueNameToString(string enumValueName, EnumNamingStrategy namingStrategy)
    {
        return namingStrategy switch
        {
            EnumNamingStrategy.MemberName => enumValueName,
            EnumNamingStrategy.CamelCase => enumValueName.ToCamelCase(),
            EnumNamingStrategy.PascalCase => enumValueName.ToPascalCase(),
            EnumNamingStrategy.SnakeCase => enumValueName.ToSnakeCase(),
            EnumNamingStrategy.UpperSnakeCase => enumValueName.ToUpperSnakeCase(),
            EnumNamingStrategy.KebabCase => enumValueName.ToKebabCase(),
            EnumNamingStrategy.UpperKebabCase => enumValueName.ToUpperKebabCase(),
            _ => throw new ArgumentOutOfRangeException($"{nameof(namingStrategy)} has an unknown value {namingStrategy}"),
        };
    }
}
