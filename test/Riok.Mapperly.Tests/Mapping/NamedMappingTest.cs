namespace Riok.Mapperly.Tests.Mapping;

public class NamedMappingTest
{
    [Fact]
    public Task IncludeMappingConfigurationOnNewInstanceMappingShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [NamedMapping("CustomName")]
            [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
            partial B MapOther(A a);

            [IncludeMappingConfiguration("CustomName")]
            partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IncludeMappingConfigurationOnExistingInstanceMappingShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [NamedMapping("CustomName")]
            [MapProperty(nameof(A.Source), nameof(B.Target))]
            public partial void MapOther(A source, B target);

            [IncludeMappingConfiguration("CustomName")]
            public partial B Map(A source);
            """,
            "class A { public string Source { get; set; } }",
            "class B { public string Target { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
