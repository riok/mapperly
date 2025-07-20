namespace Riok.Mapperly.Tests.Mapping;

public class ReferenceExternalMappingsTests
{
    [Fact]
    public Task MapValueCanUseExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(OtherMapper.NewValue))]
            internal static partial B Map(A source);

            public static string NewValue() => "new value";
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
}
