namespace Riok.Mapperly.Tests.Mapping;

public class IncludeMappingConfigurationTest
{
    [Fact]
    public Task IncludeMappingConfigurationUsesMapPropertyFromTargetMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B OtherMappingMethod(A a);

            [IncludeMappingConfiguration(nameof(OtherMappingMethod))]
            partial B MapAnother(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
