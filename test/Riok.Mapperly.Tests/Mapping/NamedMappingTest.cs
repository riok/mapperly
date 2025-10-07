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
            [MapValue(nameof(B.Value), Use = "CustomGetValue")]
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
    public Task MapPropertyWithUseUsesNamedMappingOnNewInstanceWhenNoAutoMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue), nameof(B.StringValue), Use = "CustomModifyString")]
            public partial B Map(A source);

            [NamedMapping("CustomModifyString")]
            private string ModifyString(string source) => source + "-modified";
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyWithUseUsesNamedMappingOnExistingInstanceWhenNoAutoMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Value), Use = "CustomCopyString")]
            public partial void Map(A source, B target);

            [NamedMapping("CustomCopyString")]
            private void CopyString(C source, D target) => target.StringValue = source.StringValue + "-modified";
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "class A { public C Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
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

    [Fact]
    public Task MapPropertyWithUseUsesNamedMappingWithExternalMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue), nameof(B.StringValue), Use = "OtherNamespace.OtherMapper.CustomModifyString")]
            public partial B Map(A source);
            """,
            TestSourceBuilderOptions.InBlockScopedNamespace,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        source += TestSourceBuilder.CSharp(
            """
            namespace OtherNamespace
            {
                class OtherMapper
                {
                    [NamedMapping("CustomModifyString")]
                    [UserMapping(Default = false)]
                    public static string ModifyString(string source) => source + "-modified";
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyFromSourceWithUseUsesNamedMappingWithExternalMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = "OtherNamespace.OtherMapper.CustomToFullName")]
            static partial B Map(A source);
            """,
            TestSourceBuilderOptions.InBlockScopedNamespace,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }"
        );

        source += TestSourceBuilder.CSharp(
            """
            namespace OtherNamespace
            {
                class OtherMapper
                {
                    [NamedMapping("CustomToFullName")]
                    [UserMapping(Default = false)]
                    public static string ToFullName(MapperNamespace.A x) => $"{x.FirstName} {x.LastName}";
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task IncludeMappingConfigurationUsesNamedMappingWithExternalMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [IncludeMappingConfiguration("OtherNamespace.OtherMapper.CustomName")]
            partial B Map(A a);
            """,
            "class A { public string SourceName { get; set; } }",
            "class B { public string DestinationName { get; set; } }"
        );

        source += TestSourceBuilder.CSharp(
            """
            namespace OtherNamespace
            {
                class OtherMapper
                {
                    [NamedMapping("CustomName")]
                    [MapProperty(nameof(A.SourceName), nameof(B.DestinationName))]
                    public static partial B MapOther(A a);
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }
}
