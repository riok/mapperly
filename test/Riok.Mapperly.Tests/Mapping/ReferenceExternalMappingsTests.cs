namespace Riok.Mapperly.Tests.Mapping;

public class ReferenceExternalMappingsTests
{
    [Fact]
    public Task MapPropertyFromSourceUseOnStaticSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@OtherMapper.ToFullName))]
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
    public Task MapPropertyFromSourceOnInstanceFieldUseSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            OtherMapper mapper = new();

            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@mapper.ToFullName))]
            partial B Map(A source);
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }",
            """
            class OtherMapper
            {
                public string ToFullName(A x) => $"{x.FirstName} {x.LastName}"
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyFromSourceOnInstancePropertyUseSupportsExternalMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            OtherMapper Mapper { get; } = new();

            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@Mapper.ToFullName))]
            partial B Map(A source);
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }",
            """
            class OtherMapper
            {
                public string ToFullName(A x) => $"{x.FirstName} {x.LastName}"
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
