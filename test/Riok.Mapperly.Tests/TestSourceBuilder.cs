using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests;

public static class TestSourceBuilder
{
    internal const string DefaultMapMethodName = "Map";

    /// <summary>
    /// Helper method to apply <see cref="System.Diagnostics.CodeAnalysis.StringSyntaxAttribute"/>
    /// to a given string.
    /// </summary>
    /// <param name="code">The c# code.</param>
    /// <returns>The c# code.</returns>
    public static string CSharp([StringSyntax(StringSyntax.CSharp)] string code) => code;

    public static string Mapping(
        [StringSyntax(StringSyntax.CSharp)] string fromTypeName,
        [StringSyntax(StringSyntax.CSharp)] string toTypeName,
        [StringSyntax(StringSyntax.CSharp)] params string[] types
    ) => Mapping(fromTypeName, toTypeName, null, types);

    public static string Mapping(
        [StringSyntax(StringSyntax.CSharp)] string fromTypeName,
        [StringSyntax(StringSyntax.CSharp)] string toTypeName,
        TestSourceBuilderOptions? options,
        [StringSyntax(StringSyntax.CSharp)] params string[] types
    )
    {
        return MapperWithBodyAndTypes($"partial {toTypeName} {DefaultMapMethodName}({fromTypeName} source);", options, types);
    }

    public static string MapperWithBody([StringSyntax(StringSyntax.CSharp)] string body, TestSourceBuilderOptions? options = null)
    {
        options ??= TestSourceBuilderOptions.Default;

        return CSharp(
            $$"""
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;
            using Riok.Mapperly.Abstractions.ReferenceHandling;

            {{(options.Namespace != null ? $"namespace {options.Namespace};" : string.Empty)}}

            {{BuildAttribute(options)}}
            public partial class {{options.MapperClassName}}
            {
                {{body}}
            }
            """
        );
    }

    public static string MapperWithBodyAndTypes(
        [StringSyntax(StringSyntax.CSharp)] string body,
        [StringSyntax(StringSyntax.CSharp)] params string[] types
    ) => MapperWithBodyAndTypes(body, null, types);

    public static string MapperWithBodyAndTypes(
        [StringSyntax(StringSyntax.CSharp)] string body,
        TestSourceBuilderOptions? options,
        [StringSyntax(StringSyntax.CSharp)] params string[] types
    )
    {
        var sep = Environment.NewLine + Environment.NewLine;
        return MapperWithBody(body, options) + sep + string.Join(sep, types);
    }

    public static SyntaxTree SyntaxTree([StringSyntax(StringSyntax.CSharp)] string source)
    {
        return CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default);
    }

    private static string BuildAttribute(TestSourceBuilderOptions options)
    {
        var attrs = new[]
        {
            Attribute(options.UseDeepCloning),
            Attribute(options.UseReferenceHandling),
            Attribute(options.ThrowOnMappingNullMismatch),
            Attribute(options.ThrowOnPropertyMappingNullMismatch),
            Attribute(options.AllowNullPropertyAssignment),
            Attribute(options.EnabledConversions),
            Attribute(options.PropertyNameMappingStrategy),
            Attribute(options.EnumMappingStrategy),
            Attribute(options.EnumMappingIgnoreCase),
            Attribute(options.IgnoreObsoleteMembersStrategy),
        }.WhereNotNull();

        return $"[Mapper({string.Join(", ", attrs)})]";
    }

    private static string? Attribute<T>(T? value, [CallerArgumentExpression("value")] string? expression = null)
        where T : struct, Enum =>
        value.HasValue
            ? Attribute(
                Convert.ChangeType(value.Value, Enum.GetUnderlyingType(typeof(T))).ToString() ?? throw new ArgumentNullException(),
                expression
            )
            : null;

    private static string? Attribute(bool? value, [CallerArgumentExpression("value")] string? expression = null) =>
        value.HasValue ? Attribute(value.Value ? "true" : "false", expression) : null;

    private static string? Attribute(string? value, [CallerArgumentExpression("value")] string? expression = null)
    {
        if (value == null)
            return null;

        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        return $"{expression.Split(".").Last()} = {value}";
    }
}
