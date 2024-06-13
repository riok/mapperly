using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyValueTest
{
    [Fact]
    public void StringToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", "fooBar")] partial B Map(A source);""",
            "class A;",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void StringToField()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", "fooBar")] partial B Map(A source);""",
            "class A;",
            "class B { public string StringValue; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void StringToNestedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("Nested.Value", "fooBar")] partial B Map(A source);""",
            "class A;",
            "class B { public C Nested { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Nested.Value = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void StringToNestedPropertyWithArrayAttributeCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue(new string[]{"Nested", "Value"}, "fooBar")] partial B Map(A source);""",
            "class A;",
            "class B { public C Nested { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Nested.Value = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void StringToNestedNullableProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("Nested.Value", "fooBar")] partial B Map(A source);""",
            "class A;",
            "class B { public C? Nested { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Nested ??= new();
                target.Nested.Value = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void StringToNestedPropertyWithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "Nested")]
            [MapValue("Nested.Value", "fooBar")]
            partial B Map(A source);
            """,
            "class A { public D Value { get; set; } }",
            "class B { public C Nested { get; set; } }",
            "class C { public int Id { get; set; } public string Value { get; set; } }",
            "class D { public int Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            // this diagnostic is emitted for the MapToC mapping
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member Value on the mapping target type C was not found on the mapping source type D"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Nested = MapToC(source.Value);
                target.Nested.Value = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void StringToPrivateField()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("_stringValue", "fooBar")] partial B Map(A source);""",
            TestSourceBuilderOptions.Default with
            {
                IncludedMembers = MemberVisibility.Private
            },
            "class A;",
            "class B { private string _stringValue; }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.GetStringValue() = "fooBar";
                return target;
                """
            );
    }

    [Fact]
    public void IntegerToStringPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", 10)] partial B Map(A source);""",
            "class A;",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueTypeMismatch,
                "Cannot assign constant value 10 of type int to B.StringValue of type string"
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
    public void IntegerToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", 1234)] partial B Map(A source);""",
            "class A;",
            "class B { public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.IntValue = 1234;
                return target;
                """
            );
    }

    [Fact]
    public void IntegerToCtorParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("intValue", 1234)] partial B Map(A source);""",
            "class A;",
            "class B { public B(int intValue) {} }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(1234);
                return target;
                """
            );
    }

    [Fact]
    public void IntegerToCtorParameterCaseInsensitive()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("intValue", 1234)] partial B Map(A source);""",
            "class A;",
            "class B { public B(int intValue) {} }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(1234);
                return target;
                """
            );
    }

    [Fact]
    public void IntegerToRecord()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", 1234)] partial B Map(A source);""",
            "class A;",
            "record B(int IntValue);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(1234);
                return target;
                """
            );
    }

    [Fact]
    public void IntegerToInitOnly()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", 1234)] partial B Map(A source);""",
            "class A;",
            "class B { public int IntValue { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    IntValue = 1234,
                };
                return target;
                """
            );
    }

    [Fact]
    public void IntegerToReadOnlyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", 1234)] partial B Map(A source);""",
            "class A;",
            "class B { public int IntValue { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CannotMapToReadOnlyMember, "Cannot map 1234 to read only member B.IntValue")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void TypeToPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("Value", typeof(A))] partial B Map(A source);""",
            "class A;",
            "class B { public Type Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.MapValueUnsupportedType, "The MapValueAttribute does not support types and arrays")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void ArrayToPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("Value", new int[] {1,2,3})] partial B Map(A source);""",
            "class A;",
            "class B { public int[] Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.MapValueUnsupportedType, "The MapValueAttribute does not support types and arrays")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitNullToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", null)] partial B Map(A source);""",
            "class A;",
            "class B { public string? StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = null;
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitDefaultToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", default)] partial B Map(A source);""",
            "class A;",
            "class B { public string? StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = default;
                return target;
                """
            );
    }

    [Fact]
    public void NullToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", null)] partial B Map(A source);""",
            "class A;",
            "class B { public string? StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = null;
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitNullToNonNullablePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", null)] partial B Map(A source);""",
            "class A;",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CannotMapValueNullToNonNullable,
                "Cannot assign null to non-nullable member B.StringValue of type string"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = null!;
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitDefaultToNonNullablePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("StringValue", default)] partial B Map(A source);""",
            "class A;",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CannotMapValueNullToNonNullable,
                "Cannot assign null to non-nullable member B.StringValue of type string"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = default!;
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitDefaultToValueProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", default)] partial B Map(A source);""",
            "class A;",
            "class B { public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.IntValue = default;
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitNullToValuePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", null)] partial B Map(A source);""",
            "class A;",
            "class B { public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CannotMapValueNullToNonNullable,
                "Cannot assign null to non-nullable member B.IntValue of type int"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.IntValue = default;
                return target;
                """
            );
    }

    [Fact]
    public void ExplicitNullToNullableValueProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("IntValue", null)] partial B Map(A source);""",
            "class A;",
            "class B { public int? IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.IntValue = null;
                return target;
                """
            );
    }

    [Fact]
    public void EnumToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("EnumValue", E1.Value2)] partial B Map(A source);""",
            "class A;",
            "enum E1 { Value1, Value2 }",
            "class B { public E1 EnumValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.EnumValue = global::E1.Value2;
                return target;
                """
            );
    }

    [Fact]
    public void NamespacedEnumToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("EnumValue", MyNamespace.E1.Value2)] partial B Map(A source);""",
            "class A;",
            "namespace MyNamespace { enum E1 { Value1, Value2 } }",
            "class B { public MyNamespace.E1 EnumValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.EnumValue = global::MyNamespace.E1.Value2;
                return target;
                """
            );
    }

    [Fact]
    public void EnumTypeMismatchShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("EnumValue", 1)] partial B Map(A source);""",
            "class A;",
            "enum E1 { Value1, Value2 }",
            "class B { public E1 EnumValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueTypeMismatch,
                "Cannot assign constant value 1 of type int to B.EnumValue of type E1"
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
    public void MapValueDuplicateForSameTargetMemberShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", 10)]
            [MapValue("Value", 20)]
            partial B Map(A source);
            """,
            "class A;",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                "Multiple mappings are configured for the same target member B.Value"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = 10;
                return target;
                """
            );
    }

    [Fact]
    public void MapValueAndPropertyAttributeForSameTargetShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", 10)]
            [MapProperty("SourceValue", "Value")]
            partial B Map(A source);
            int NewValue() => 11;
            """,
            "class A { public int SourceValue { get; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MultipleConfigurationsForTargetMember,
                "Multiple mappings are configured for the same target member B.Value"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member SourceValue on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.Value = 10;
                return target;
                """
            );
    }

    [Fact]
    public void StringToInitOnlyPathShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """[MapValue("Nested.Value", "fooBar")] partial B Map(A source);""",
            "class A;",
            "class B { public C Nested { get; init; } }",
            "class C { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InitOnlyMemberDoesNotSupportPaths, "Cannot map to init only member path B.Nested.Value")
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotFound,
                "The member Nested on the mapping target type B was not found on the mapping source type A"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }
}
