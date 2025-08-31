using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Tests;

public static class TestSourceBuilder
{
    public const string DefaultNamespace = "MapperNamespace";

    private const string DefaultUsingDirectives = """
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using Riok.Mapperly.Abstractions;
        using Riok.Mapperly.Abstractions.ReferenceHandling;
        """;

    internal const string DefaultMapMethodName = "Map";

    private static readonly string _newlines = Environment.NewLine + Environment.NewLine;

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
        return MapperWithBodyAndTypes($"private partial {toTypeName} {DefaultMapMethodName}({fromTypeName} source);", options, types);
    }

    public static string MapperWithBody([StringSyntax(StringSyntax.CSharp)] string body, TestSourceBuilderOptions? options = null)
    {
        options ??= TestSourceBuilderOptions.Default;
        var additionalUsings = AdditionalUsings(options);

        return CSharp(
            $$"""
              {{DefaultUsingDirectives}}{{additionalUsings}}
              {{(options.Namespace != null ? $"namespace {options.Namespace};" : "")}}

              {{BuildAttribute(options)}}
              public {{(options.Static ? "static " : "")}}partial class {{options.MapperClassName}}{{(
                  options.MapperBaseClassName != null ? " : " + options.MapperBaseClassName : ""
              )}}
              {
                  {{body}}
              }
              """
        );
    }

    public static string MapperWithBodyInBlockScopedNamespace(
        [StringSyntax(StringSyntax.CSharp)] string body,
        TestSourceBuilderOptions? options = null
    )
    {
        options ??= TestSourceBuilderOptions.Default;
        var additionalUsings = AdditionalUsings(options);

        return CSharp(
            $$"""
              {{DefaultUsingDirectives}}{{additionalUsings}}

              namespace {{options.Namespace ?? DefaultNamespace}} {

                  {{BuildAttribute(options)}}
                  public {{(options.Static ? "static " : "")}}partial class {{options.MapperClassName}}{{(
                      options.MapperBaseClassName != null ? " : " + options.MapperBaseClassName : ""
                  )}}
                  {
                      {{body}}
                  }
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
        return $"{MapperWithBody(body, options)}{_newlines}{string.Join(_newlines, types)}";
    }

    public static string Append(string source, [StringSyntax(StringSyntax.CSharp)] string[] classes)
    {
        return Append(source, DefaultNamespace, classes);
    }

    public static string Append(string source, string @namespace, [StringSyntax(StringSyntax.CSharp)] string[] classes)
    {
        var newClasses = string.Join(_newlines, classes);

        var newSource = $$"""
            namespace {{@namespace}} {

                {{newClasses}}
            }
            """;

        return $"{source}{_newlines}{newSource}";
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
            Attribute(options.RequiredMappingStrategy),
            Attribute(options.RequiredEnumMappingStrategy),
            Attribute(options.IncludedMembers),
            Attribute(options.IncludedConstructors),
            Attribute(options.PreferParameterlessConstructors),
            Attribute(options.AutoUserMappings),
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

    private static string AdditionalUsings(TestSourceBuilderOptions options)
    {
        var additionalUsings =
            options.AdditionalUsings != null ? string.Join(_newlines, options.AdditionalUsings.Select(u => $"using {u};")) : string.Empty;
        return Environment.NewLine + additionalUsings;
    }
}
