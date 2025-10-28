using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Configuration;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBodyBuilders.BuilderContext;

public static class EnumMappingBuilder
{
    internal static string GetMemberName(MappingBuilderContext ctx, IFieldSymbol field) =>
        GetMemberName(ctx, field, ctx.Configuration.Enum.NamingStrategy);

    private static string GetMemberName(MappingBuilderContext ctx, IFieldSymbol field, EnumNamingStrategy namingStrategy)
    {
        return namingStrategy switch
        {
            EnumNamingStrategy.MemberName => field.Name,
            EnumNamingStrategy.CamelCase => field.Name.ToCamelCase(),
            EnumNamingStrategy.PascalCase => field.Name.ToPascalCase(),
            EnumNamingStrategy.SnakeCase => field.Name.ToSnakeCase(),
            EnumNamingStrategy.UpperSnakeCase => field.Name.ToUpperSnakeCase(),
            EnumNamingStrategy.KebabCase => field.Name.ToKebabCase(),
            EnumNamingStrategy.UpperKebabCase => field.Name.ToUpperKebabCase(),
            EnumNamingStrategy.ComponentModelDescriptionAttribute => GetComponentModelDescription(ctx, field),
            EnumNamingStrategy.SerializationEnumMemberAttribute => GetEnumMemberValue(ctx, field),
            _ => throw new ArgumentOutOfRangeException($"{nameof(namingStrategy)} has an unknown value {namingStrategy}"),
        };
    }

    private static string GetEnumMemberValue(MappingBuilderContext ctx, IFieldSymbol field)
    {
        var name = ctx.AttributeAccessor.ReadEnumMemberAttribute(field)?.Value;
        if (name != null)
            return name;

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumNamingAttributeMissing,
            nameof(EnumMemberAttribute),
            field.Name,
            field.ConstantValue ?? "<unknown>"
        );
        return GetMemberName(ctx, field, EnumNamingStrategy.MemberName);
    }

    private static string GetComponentModelDescription(MappingBuilderContext ctx, IFieldSymbol field)
    {
        var name = ctx.AttributeAccessor.ReadDescriptionAttribute(field)?.Description;
        if (name != null)
            return name;

        ctx.ReportDiagnostic(
            DiagnosticDescriptors.EnumNamingAttributeMissing,
            nameof(DescriptionAttribute),
            field.Name,
            field.ConstantValue ?? "<unknown>"
        );
        return GetMemberName(ctx, field, EnumNamingStrategy.MemberName);
    }
}
