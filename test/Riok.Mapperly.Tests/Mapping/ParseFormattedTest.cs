namespace Riok.Mapperly.Tests.Mapping;

public class ParseFormattedTest
{
    [Fact]
    public void ParseableCustomClassWithFormatProviderFormatProviderNotDefined()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            "class A { public static A Parse(string v, System.IFormatProvider? f) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Parse(source, null);");
    }

    [Fact]
    public void ParseableCustomClassWithFormatProviderDefaultFormatProviderDefined()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private readonly IFormatProvider _formatter = CultureInfo.GetCultureInfo("de-CH");

            private partial A Map(string source);
            """,
            "class A { public static A Parse(string v, System.IFormatProvider? f) => new(); }"
        );

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Parse(source, _formatter);");
    }

    [Fact]
    public void ParseableCustomClassWithFormatProviderNamedFormatProviderDefined()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [FormatProvider(Default = true)]
            private IFormatProvider CurrentCulture => CultureInfo.CurrentCulture;

            [FormatProvider]
            private readonly IFormatProvider _enCulture = CultureInfo.GetCultureInfo("en-US");

            [MapProperty(nameof(A.BValue), nameof(C.BValue), FormatProvider = nameof(_enCulture))]
            private partial C Map(A source);
            """,
            "class A { public string BValue { get; set; } }",
            "class B { public static A Parse(string v, System.IFormatProvider? f) => new(); }",
            "class C { public B BValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::C();
                target.BValue = global::B.Parse(source.BValue, _enCulture);
                return target;
                """
            );
    }
}
