using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ToStringFormattedTest
{
    [Fact]
    public void ClassMultiplePropertiesToStringWithDifferentFormats()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value1", "Value1", StringFormat = "dd.MM.yyyy")]
            [MapProperty("Value2", "Value2", StringFormat = "yyyy-MM-dd")]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } public DateTime Value1 { get; set; } public DateTime Value2 { get; set; } }",
            "class B { public string Value { get; set; } public string Value1 { get; set; } public string Value2 { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString();
                target.Value1 = source.Value1.ToString("dd.MM.yyyy");
                target.Value2 = source.Value2.ToString("yyyy-MM-dd");
                return target;
                """
            );
    }

    [Fact]
    public void RecordMultiplePropertiesToStringWithDifferentFormats()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value1", "Value1", StringFormat = "dd.MM.yyyy")]
            [MapProperty("Value2", "Value2", StringFormat = "yyyy-MM-dd")]
            partial B Map(A source);",
            """,
            "record A(DateTime Value, DateTime Value1, DateTime Value2);",
            "record B(string Value, string Value1, string Value2);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(
                    source.Value.ToString(),
                    source.Value1.ToString("dd.MM.yyyy"),
                    source.Value2.ToString("yyyy-MM-dd")
                );
                return target;
                """
            );
    }

    [Fact]
    public void WithBothParamsButOnlyFormatProviderSet()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", FormatProvider = nameof(_formatter)]
            partial B Map(A source);",
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C { public string ToString(string? format, IFormatProvider? formatProvider) => format; }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString(null, _formatter);
                return target;
                """
            );
    }

    [Fact]
    public void WithBothParamsButOnlyFormatSet()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "Value", StringFormat = "C")]
            partial B Map(A source);",
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C { public string ToString(string? format, IFormatProvider? formatProvider) => format; }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("C", null);
                return target;
                """
            );
    }

    [Fact]
    public void DefaultFormatProvider()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString(_formatter);
                return target;
                """
            );
    }

    [Fact]
    public void DefaultFormatProviderButNoFormatProviderOverload()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");
            partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C { public string ToString(string? format) => format; }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString();
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitFormatProviderButNoFormatProviderOverloadShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", FormatProvider = nameof(_formatter)]
            partial B Map(A source);",
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C { public string ToString(string? format) => format; }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceDoesNotImplementToStringWithFormatParameters,
                "The source type C does not implement ToString with the provided formatting parameters, string format and format provider cannot be applied"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DefaultFormatProviderAndStringFormat()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", StringFormat = "dd.MM.yyyy")]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("dd.MM.yyyy", _formatter);
                return target;
                """
            );
    }

    [Fact]
    public void DefaultFormatProviderAndStringFormatButNoFormatProviderOverload()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", StringFormat = "C")]
            partial B Map(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C { public string ToString(string? format) => format; }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("C");
                return target;
                """
            );
    }

    [Fact]
    public void DefaultAndExplicitFormatProviderAndStringFormat()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [FormatProvider]
            private readonly IFormatProvider _formatterEN = CultureInfo.GetCultureInfo("en-US");

            [MapProperty("Value", "Value", StringFormat = "yyyy-MM-dd", FormatProvider = nameof(_formatterEN))]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("yyyy-MM-dd", _formatterEN);
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitFormatProvider()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider]
            private readonly IFormatProvider _formatterEN = CultureInfo.GetCultureInfo("en-US");

            [MapProperty("Value", "Value", StringFormat = "yyyy-MM-dd", FormatProvider = nameof(_formatterEN))]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } public DateTime Value2 { get; set; } }",
            "class B { public string Value { get; set; } public string Value2 { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("yyyy-MM-dd", _formatterEN);
                target.Value2 = source.Value2.ToString();
                return target;
                """
            );
    }

    [Fact]
    public void DefaultFormatProviderDisabledNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::B();
                target.Value = source.Value.ToString(_formatter);
                return target;
                """
            );
    }

    [Fact]
    public void NullableFormatProvider()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider? _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", StringFormat = "dd.MM.yyyy")]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("dd.MM.yyyy", _formatter);
                return target;
                """
            );
    }

    [Fact]
    public void FormatProviderStaticInStaticMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private static readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");
            static partial B Map(A source);
            """,
            TestSourceBuilderOptions.AsStatic,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString(_formatter);
                return target;
                """
            );
    }

    [Fact]
    public void FormatProviderStaticInStaticMethodsMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private static readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");
            static partial B Map(A source);
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString(_formatter);
                return target;
                """
            );
    }

    [Fact]
    public void FormatProviderStaticInMapperShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private static readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");
            partial B Map(A source);
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidFormatProviderSignature, "The format provider _formatter has an invalid signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void FormatProviderStaticShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private static readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", StringFormat = "dd.MM.yyyy")]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidFormatProviderSignature, "The format provider _formatter has an invalid signature")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value.ToString("dd.MM.yyyy");
                return target;
                """
            );
    }

    [Fact]
    public void UnknownFormatProviderShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [MapProperty("Value", "Value", FormatProvider = "fooBar")]
            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.FormatProviderNotFound,
                "The format provider fooBar could not be found, make sure it is annotated with FormatProviderAttribute"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DuplicatedDefaultFormatProviderShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatterDe = CultureInfo.GetCultureInfo("de-DE");

            partial B Map(A source);",
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MultipleDefaultFormatProviders,
                "Multiple default format providers found, only one is allowed"
            )
            .HaveAssertedAllDiagnostics();
    }
}
