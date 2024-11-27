using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyTest
{
    [Fact]
    public void OneSimpleProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void SameType()
    {
        var source = TestSourceBuilder.Mapping("A", "A", "class A { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void SameTypeDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.StringValue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void CustomRefStructToSameCustomStruct()
    {
        var source = TestSourceBuilder.Mapping("A", "A", "ref struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void CustomRefStructToSameCustomStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("A", "A", TestSourceBuilderOptions.WithDeepCloning, "ref struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void StringToIntProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = int.Parse(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public Task ShouldIgnoreWriteOnlyPropertyOnSourceWithDiagnostics()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValue2 { set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldIgnoreReadOnlyPropertyWhenMatchedAutomatically()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValue2 { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ShouldIgnoreIndexedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int this[int index] { get => -1; set { } } }",
            "class B { public int this[int index] { get => -1; set { } } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAndIncludeAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoMemberMappings, "No members are mapped in the object mapping from A to B")
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public Task WithUnmatchedPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValueA { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValueB { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithManualMappedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue), nameof(B.StringValue2)] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue2 = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void WithManualMappedPropertyDuplicatedAndNullFilter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.StringValue1), nameof(B.StringValue)]
            [MapProperty(nameof(A.StringValue2), nameof(B.StringValue)]
            partial B Map(A source);
            """,
            TestSourceBuilderOptions.Default with
            {
                AllowNullPropertyAssignment = false,
            },
            "class A { public string? StringValue1 { get; set; } public string? StringValue2 { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property StringValue1 of A to the target property StringValue of B which is not nullable"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property StringValue2 of A to the target property StringValue of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                if (source.StringValue1 != null)
                {
                    target.StringValue = source.StringValue1;
                }
                if (source.StringValue2 != null)
                {
                    target.StringValue = source.StringValue2;
                }
                return target;
                """
            );
    }

    [Fact]
    public void WithPropertyNameMappingStrategyCaseInsensitive()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);",
            new TestSourceBuilderOptions { PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive },
            "class A { public string StringValue { get; set; } public int Value { get; set; } }",
            "class B { public string stringvalue { get; set; } public required int value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    value = source.Value,
                };
                target.stringvalue = source.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public Task WithPropertyNameMappingStrategyCaseSensitive()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial B Map(A source);",
            new TestSourceBuilderOptions { PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseSensitive },
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string stringvalue { get; set; } public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithManualMappedNotFoundTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue), nameof(B.StringValue9)] private partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithManualMappedNotFoundSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"StringValue9\", nameof(B.StringValue2)] private partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ShouldUseUserImplementedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial B Map(A source);

            [UserMapping(Default = true)]
            private D UserImplementedMap(C source)
            {
                var target = Map(source);
                target.StringValue += "ok";
                return target;
            }

            private partial D MapToD(C source);
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                target.NestedValue = UserImplementedMap(source.NestedValue);
                return target;
                """
            );
    }

    [Fact]
    public Task WithUnmappablePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public Version Value { get; set; } }",
            "class B { public DateTime Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void NullableToNonNullablePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string? StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceValueToNonNullableTargetValue,
                "Mapping the nullable source property StringValue of A to the target property StringValue of B which is not nullable"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task WithManualNotFoundTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue), \"not_found\")] B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithManualNotFoundSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"not_found\", nameof(B.StringValue2))] private partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void PrivateMemberPropertyShouldNotOverride()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { private int MyValue { get; set; } public C My { get; set; } }",
            "class B { public int MyValue { get; set; } }",
            "class C { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.MyValue = source.My.Value;
                return target;
                """
            );
    }

    [Fact]
    public Task WithPrivateSourceGetterShouldIgnoreAndDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { private get; set; } public int IntValue { get; private set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { private get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithPrivateSourcePathGetterShouldIgnoreAndDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C NestedValue { private get; set; } public int IntValue { get; private set; } }",
            "class B { public D NestedValue { get; set; } public int IntValue { private get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void UnmappedReadOnlyTargetPropertyShouldNotDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Name { get; } }",
            "class B { public string Name { set; } public string FullName { get; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Name = source.Name;
                return target;
                """
            );
    }

    [Fact]
    public Task PropertiesWithCaseInsensitiveEqualNamesShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int Value { get; set; } }",
            "class B { public int value { get; set; } public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PropertyConfigurationShouldPreferExactCasing()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "value")]
            [MapProperty("value", "Value")]
            public partial B Map(A source);
            """,
            "class A { public int value { get; set; } public int Value { get; set; } }",
            "class B { public int value { get; set; } public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ShouldIgnoreStaticProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Name { get; } public static string Value { get; } }",
            "class B { public string Name { set; } public static string Value { set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Name = source.Name;
                return target;
                """
            );
    }

    [Fact]
    public void ShouldIgnoreStaticConstructorAndDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } }",
            "class B { static B() {} private B() {} public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoConstructorFound, "B has no accessible constructor with mappable arguments")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void ModifyingTemporaryStructShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("StringValue", "NestedValue.StringValue")]
            partial B Map(A src);
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public C NestedValue { get; set; } }",
            "struct C { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member StringValue on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member NestedValue on the mapping target type B was not found on the mapping source type A"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member StringValue on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member NestedValue on the mapping target type B was not found on the mapping source type A"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.CannotMapToTemporarySourceMember,
                "Cannot map from member A.StringValue to member path B.NestedValue.StringValue of type string because C.NestedValue is a value type, returning a temporary value, see CS1612"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void ModifyingPathIfClassPrecedesShouldNotDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("StringValue", "NestedValue.StringValue")]
            partial B Map(A src);
            """,
            "class A { public string StringValue { get; set; } }",
            "struct B { public C NestedValue { get; set; } }",
            "class C { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.NestedValue.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void InvalidMapPropertyAttributeUsageShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("IntValue", "StringValue", StringFormat = "D", Use = nameof(IntToString))]
            partial B Map(A src);

            string IntToString(int x) => x.ToString();
            """,
            "class A { public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidMapPropertyAttributeUsage, "Invalid usage of the MapPropertyAttribute")
            .HaveAssertedAllDiagnostics();
    }
}
