using System.Runtime.CompilerServices;

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
        var attrs = new[]
        {
            Attribute(options.UseDeepCloning),
            Attribute(options.ThrowOnMappingNullMismatch),
            Attribute(options.ThrowOnPropertyMappingNullMismatch),
            Attribute(options.EnabledConversions),
            Attribute(options.PropertyNameMappingStrategy),
        };

        return $"[Mapper({string.Join(", ", attrs)})]";
    }

    private static string Attribute<T>(T value, [CallerArgumentExpression("value")] string? expression = null)
        where T : Enum
        => Attribute(Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T))).ToString() ?? throw new ArgumentNullException(), expression);

    private static string Attribute(bool value, [CallerArgumentExpression("value")] string? expression = null)
        => Attribute(value ? "true" : "false", expression);

    private static string Attribute(string value, [CallerArgumentExpression("value")] string? expression = null)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        return $"{expression.Split(".").Last()} = {value}";
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