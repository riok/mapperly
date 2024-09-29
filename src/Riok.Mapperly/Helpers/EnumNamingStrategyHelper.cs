using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Helpers;

public static class EnumNamingStrategyHelper
{
    public static string GetName(this IFieldSymbol field, EnumNamingStrategy namingStrategy) =>
        namingStrategy switch
        {
            EnumNamingStrategy.MemberName => field.Name,
            EnumNamingStrategy.CamelCase => field.Name.ToCamelCase(),
            EnumNamingStrategy.PascalCase => field.Name.ToPascalCase(),
            EnumNamingStrategy.SnakeCase => field.Name.ToSnakeCase(),
            EnumNamingStrategy.UpperSnakeCase => field.Name.ToUpperSnakeCase(),
            EnumNamingStrategy.KebabCase => field.Name.ToKebabCase(),
            EnumNamingStrategy.UpperKebabCase => field.Name.ToUpperKebabCase(),
            _ => throw new ArgumentOutOfRangeException($"{nameof(namingStrategy)} has an unknown value {namingStrategy}"),
        };
}
