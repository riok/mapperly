namespace Riok.Mapperly.Tests.Mapping;

public class ReferenceExternalMappingsTests
{
    [Fact]
    public Task IncludeMappingConfigurationNameSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [IncludeMappingConfiguration(nameof(@OtherMapper.MapOther))]
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

    [Fact]
    public Task ExternalMappingWorksWithFullNamespacePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(@Some.Namespace.OtherMapper.NewValue))]
            internal static partial B Map(A source);
            """,
            "class A;",
            "record B(string Value);",
            """
            namespace Some.Namespace
            {
                class OtherMapper
                {
                    public static string NewValue() => "new value";
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
