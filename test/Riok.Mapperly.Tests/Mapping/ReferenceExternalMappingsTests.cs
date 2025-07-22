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

    [Fact]
    public Task MapPropertyFromSourceUseSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(OtherMapper.ToFullName))]
            partial B Map(A source);
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }",
            """
            class OtherMapper
            {
                public static string ToFullName(A x) => $"{x.FirstName} {x.LastName}"
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IncludeMappingConfigurationNameSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [IncludeMappingConfiguration(nameof(OtherMapper.MapOther))]
            static partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }",
            """
            class OtherMapper {
                [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
                public static partial B MapOther(A a);
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
