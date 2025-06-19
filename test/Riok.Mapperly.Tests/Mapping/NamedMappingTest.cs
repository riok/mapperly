namespace Riok.Mapperly.Tests.Mapping;

public class NamedMappingTest
{
    [Fact]
    public Task IncludeMappingConfigurationOnNewInstanceMappingUsesNamedMapping()
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
    public Task IncludeMappingConfigurationOnExistingInstanceMappingUsesNamedMapping()
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

    [Fact]
    public Task MapValueWithUseUsesNamedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue(nameof(A.Value), Use = "CustomGetValue")]
            partial B Map(A a);

            [NamedMapping("CustomGetValue")]
            string GetValue() => "C1";
            """,
            "class A { }",
            "class B { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyWithUseUsesNamedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue), nameof(B.StringValue), Use = "CustomModifyString")]
            public partial B Map(A source);

            [NamedMapping("CustomModifyString")]
            [UserMapping(Default = false)]
            private string ModifyString(string source) => source + "-modified";
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyFromSourceWithUseUsesNamedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = "CustomToFullName")]
            partial B Map(A source);

            [NamedMapping("CustomToFullName")]
            [UserMapping(Default = false)]
            string ToFullName(A x) => $"{x.FirstName} {x.LastName}";
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
