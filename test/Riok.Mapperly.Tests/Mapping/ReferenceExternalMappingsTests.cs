namespace Riok.Mapperly.Tests.Mapping;

public class ReferenceExternalMappingsTests
{
    [Fact]
    public Task MapValueUseSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(OtherMapper.NewValue))]
            internal static partial B Map(A source);
            """,
            "class A;",
            "record B(string Value);",
            """
            class OtherMapper
            {
                public static string NewValue() => "new value";
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyUseSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value1), nameof(B.Value1), Use = nameof(ModifyString)]
            [MapProperty(nameof(A.Value2), nameof(B.Value2), Use = nameof(OtherMapper.ModifyString)]
            private static partial B Map(A source);

            public static string ModifyString(string source) => source + "-modified";
            """,
            "record A(string Value1, string Value2);",
            "record B(string Value1, string Value2);",
            """
            class OtherMapper
            {
                public static string ModifyString(string source) => source + "-externally-modified";
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
