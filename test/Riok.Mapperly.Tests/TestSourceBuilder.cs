using System.Runtime.CompilerServices;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests;

public static class TestSourceBuilder
{
    internal const string DefaultMapMethodName = "Map";

    public static string Mapping(string fromTypeName, string toTypeName, params string[] types)
        => Mapping(fromTypeName, toTypeName, null, types);

    public static string Mapping(string fromTypeName, string toTypeName, TestSourceBuilderOptions? options, params string[] types)
    {
        return MapperWithBodyAndTypes(
            $"partial {toTypeName} {DefaultMapMethodName}({fromTypeName} source);",
            options,
            types);
    }

    public static string MapperWithBody(string body, TestSourceBuilderOptions? options = null)
    {
        options ??= TestSourceBuilderOptions.Default;

        return $@"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

{(options.Namespace != null ? $"namespace {options.Namespace};" : string.Empty)}

{BuildAttribute(options)}
public partial class Mapper
{{
    {body}
}}
";
    }

    private static string BuildAttribute(TestSourceBuilderOptions options)
    {
        var attrs = new List<string>
        {
            Attribute(options.UseDeepCloning),
            Attribute(options.ThrowOnMappingNullMismatch),
            Attribute(options.ThrowOnPropertyMappingNullMismatch)
        };

        if (options.PropertyNameMappingStrategy != PropertyNameMappingStrategy.CaseSensitive)
        {
            attrs.Add($"{nameof(MapperAttribute.PropertyNameMappingStrategy)} = {nameof(PropertyNameMappingStrategy)}.{options.PropertyNameMappingStrategy}");
        }

        return $"[Mapper({string.Join(", ", attrs)})]";
    }

    private static string Attribute(bool value, [CallerArgumentExpression("value")] string? expression = null)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        return $"{expression.Split(".").Last()} = {(value ? "true" : "false")}";
    }

    public static string MapperWithBodyAndTypes(string body, params string[] types)
        => MapperWithBodyAndTypes(body, null, types);

    public static string MapperWithBodyAndTypes(string body, TestSourceBuilderOptions? options, params string[] types)
    {
        var sep = Environment.NewLine + Environment.NewLine;
        return MapperWithBody(body, options)
            + sep
            + string.Join(sep, types);
    }
}
