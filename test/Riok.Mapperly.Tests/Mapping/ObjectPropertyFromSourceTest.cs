using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyFromSourceTest
{
    [Fact]
    public void WithManualMappedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapPropertyFromSource(nameof(B.Value)] partial B Map(A source);",
            "class A { }",
            "class B { public A Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source;
                return target;
                """
            );
    }

    [Fact]
    public void WithManualMappedNotFoundTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapPropertyFromSource(\"Value\")] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound,
                "Specified member Value on mapping target type B was not found"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MultipleToStringWithDifferentFormats()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.Time), StringFormat = "HH:mm:ss")]
            [MapPropertyFromSource(nameof(B.Date), StringFormat = "yyyy-MM-dd")]
            partial B Map(DateTime source);",
            """,
            "class B { public string Time { get; set; } public string Date { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Time = source.ToString("HH:mm:ss");
                target.Date = source.ToString("yyyy-MM-dd");
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseReferencedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FloatValue), Use = nameof(MakeFloat)]
            partial B Map(A source);

            [UserMapping(Default = false)]
            float MakeFloat(A x) => 3f * x.IntValue + x.Offset;
            """,
            "class A { public int IntValue { get; set; } public int Offset { get; set; } }",
            "class B { public float FloatValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.FloatValue = MakeFloat(source);
                return target;
                """
            );
    }

    [Fact]
    public void ShouldUseUserMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.Value)]
            partial B Map(A source);

            [UserMapping(Default = true)]
            C ConvertAToC(A x) => new C { FloatValue = 3f * x.IntValue + x.Offset };
            """,
            "class A { public int IntValue { get; set; } public int Offset { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C { public float FloatValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = ConvertAToC(source);
                return target;
                """
            );
    }

    [Fact]
    public void ToConstructorParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapPropertyFromSource(\"value\")] partial B Map(A source);",
            "class A { }",
            "class B { public B(A value) {} }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                return new global::B(source);
                """
            );
    }

    [Fact]
    public void ToInitOnlyProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapPropertyFromSource(nameof(B.Value))] partial B Map(A source);",
            "class A { }",
            "class B { public A Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    Value = source,
                };
                return target;
                """
            );
    }

    [Fact]
    public void ToTuple()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapPropertyFromSource(\"Original\")] partial (int IntValue, A Original) Map(A source);",
            "class A { public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = (IntValue: source.IntValue, Original: source);
                return target;
                """
            );
    }

    [Fact]
    public void NullableNestedPropertyWithMemberNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapPropertyFromSource(nameof(B.Value))] partial B Map(A? source);",
            new TestSourceBuilderOptions { ThrowOnMappingNullMismatch = false },
            "class A { }",
            "class B { public A? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type A? to target of type B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return new global::B();
                var target = new global::B();
                target.Value = source;
                return target;
                """
            );
    }

    [Fact]
    public Task ShouldSupportExternalMappingsOnStaticMapping()
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
                public static string ToFullName(A x) => $"{x.FirstName} {x.LastName}";
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldSupportExternalMappingsWhenAutoUserMappingsDisabled()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@OtherMapper.ToFullName))]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.WithDisabledAutoUserMappings,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }",
            """
            class OtherMapper
            {
                public static string ToFullName(A x) => $"{x.FirstName} {x.LastName}";
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldSupportExternalMappingsOnInstanceField()
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
    public Task ShouldSupportExternalMappingsOnInstanceProperty()
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
    public Task ShouldSupportExternalMappingsOnInstancePropertyWithString()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            OtherMapper Property { get; } = new();

            [MapPropertyFromSource(nameof(B.FullName), Use = "Property.ToFullName")]
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
    public Task ShouldSupportExternalMappingsOnString()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = "OtherNamespace.OtherMapper.ToFullName")]
            partial B Map(A source);
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
                    public static string ToFullName(MapperNamespace.A x) => $"{x.FirstName} {x.LastName}";
                }
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldReportNonExistentStaticExternalMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@OtherMapper.ToFullName))]
            partial B Map(A source);
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }",
            "class OtherMapper;"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldReportNonExistentExternalMappingsOnInstanceReference()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            OtherMapper mapper = new();

            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@mapper.ToFullName))]
            partial B Map(A source);
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }",
            "class OtherMapper;"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldReportNonExistentInstanceFieldReference()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = nameof(@mapper.ToFullName))]
            partial B Map(A source);
            """,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldReportNonExistentExternalMappingsOnString()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.FullName), Use = "OtherNamespace.OtherMapper.ToFullName")]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.InBlockScopedNamespace,
            "class A { public string FirstName { get; set; } public string LastName { get; set; } }",
            "class B { public string FullName { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
