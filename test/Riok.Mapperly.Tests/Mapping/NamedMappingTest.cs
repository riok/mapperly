namespace Riok.Mapperly.Tests.Mapping;

public class NamedMappingTest
{
    [Fact]
    public Task IncludeMappingConfigurationUsesNamedAlias()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [NamedMapping("mapperAlias")]
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration("mapperAlias")]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
