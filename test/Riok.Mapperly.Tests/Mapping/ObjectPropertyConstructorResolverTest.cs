namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectPropertyConstructorResolverTest
{
    [Fact]
    public void ClassToClassWithOneMatchingCtor()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue) {} { public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public Task ClassToClassWithOneMatchingCtorAndUnmatchedSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue) {} { }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ClassToClassWithOneMatchingCtorWithMatchedOptional()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue, int intValue = 10) {} }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue, source.IntValue);
                return target;
                """
            );
    }

    [Fact]
    public void ClassToClassWithOneMatchingCtorWithUnmatchedOptional()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue, int intValue2 = 10) {} public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToClassWithOneMatchingCtorWithMatchedAndUnmatchedOptionals()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } public int IntValue3 { get; set; } }",
            "class B { public B(string stringValue, int intValue2 = 10, int intValue = 20) {} public int IntValue3 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue, intValue: source.IntValue);
                target.IntValue3 = source.IntValue3;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldChooseSingleMatching()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string x) { } public B(string stringValue, int intvalue) { } public B(string stringValue) { } { public int IntValue { get; set; } "
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue, source.IntValue);
                return target;
                """
            );
    }

    [Fact]
    public Task ClassToClassPrivateCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { private B(){} }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUnmatchedCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string StringValue9){} public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUnmatchedAttributedCtorShouldDiagnosticAndUseAlternative()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { [MapperConstructor] public B(string StringValue9){} public B(string StringValue) {} public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldUseAttributed()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string x) { } [MapperConstructor] public B(string stringValue) { } public B() { } { public string StringValue { get; set; } public int IntValue { get; set; } "
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldPreferParameterless()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string x) { } public B(string stringValue, int intvalue) { } public B() { } { public string StringValue { get; set; } public int IntValue { get; set; } "
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.StringValue = source.StringValue;
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldPreferNonObsolete()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { [Obsolete] public B(string stringValue, int intValue) { } public B(string stringValue) { } { public string StringValue { get; set; } public int IntValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldPreferObsoleteWithMapperCtorAttribute()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { [Obsolete, MapperConstructor] public B(string StringValue) { } public B(string stringValue, int intvalue) { } { public string StringValue { get; set; } public int IntValue { get; set; } "
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecord()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(string StringValue) { public int IntValue { get; set; } }",
            "record B(string StringValue) { public int IntValue { get; set; } "
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToRecord()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "record B(string StringValue) { public int IntValue { get; set; } "
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.StringValue);
                target.IntValue = source.IntValue;
                return target;
                """
            );
    }

    [Fact]
    public void RecordToFlattenedRecord()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "record A(C Nested);", "record B(string NestedValue);", "record C(string Value);");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Nested.Value);
                return target;
                """
            );
    }

    [Fact]
    public void RecordToFlattenedRecordNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(C? Nested);",
            "record B(string NestedValue);",
            "record C(string Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Nested?.Value ?? throw new System.ArgumentNullException(nameof(source.Nested.Value)));
                return target;
                """
            );
    }

    [Fact]
    public void RecordToFlattenedRecordNullablePathWithCustomMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);" + "string StrConversion(string? s) => s ?? \"\";",
            "record A(C? Nested);",
            "record B(string NestedValue);",
            "record C(string Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(StrConversion(source.Nested?.Value));
                return target;
                """
            );
    }

    [Fact]
    public void RecordToFlattenedRecordNullablePathNoThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnMappingNullMismatch = false
            },
            "record A(C? Nested);",
            "record B(string NestedValue);",
            "record C(string Value);"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Nested?.Value ?? "");
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecordNonNullableToNullablePrimitive()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "record A(int Value);", "record B(int? Value);");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecordNullableToNonNullablePrimitive()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "record A(int? Value);", "record B(int Value);");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value ?? throw new System.ArgumentNullException(nameof(source.Value)));
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecordNonDirectAssignmentNullHandling()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "record A(int? Value);", "record B(double Value);");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value != null ? (double)source.Value.Value : throw new System.ArgumentNullException(nameof(source.Value.Value)));
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecordFlattenedNonDirectAssignmentNullHandling()
    {
        var source = TestSourceBuilder.Mapping("A", "B", "record A(C? Nested);", "record B(double NestedValue);", "record C(int Value);");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Nested != null ? (double)source.Nested.Value : throw new System.ArgumentNullException(nameof(source.Nested.Value)));
                return target;
                """
            );
    }

    [Fact]
    public void CanResolveToRecordConstructorWithMapPropertyAttribute()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.Id), nameof(B.Id2))] partial B ToRecord(A a);",
            "class A { public string? Id { get; set; } public bool F { get; set; } }",
            "record B(string? Id2, bool F);"
        );

        var result = TestHelper.GenerateMapper(source);
        result
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(a.Id, a.F);
                return target;
                """
            );
    }

    [Fact]
    public void CanResolveToClassConstructorWithMapPropertyAttribute()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.Id), \"id2\")] partial B ToRecord(A a);",
            "class A { public string? Id { get; set; } public bool F { get; set; } }",
            "class B { public B(string? id2, bool f) { Id2 = id2; F = f; } public string? Id2 { get; set; } public bool F { get; set; } }"
        );

        var result = TestHelper.GenerateMapper(source);
        result
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(a.Id, a.F);
                return target;
                """
            );
    }

    [Fact]
    public void CanResolveToClassPrimaryConstructor()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A(string? id, bool value) { public string? Id => id; public bool Value => value; }",
            "class B(string? id, bool value) {}"
        );

        var result = TestHelper.GenerateMapper(source);
        result
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Id, source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecordNullableToNullableEnum()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(C? EnumValue);",
            "record B(D? EnumValue);",
            "enum C { Value1, Value2 }",
            "enum D { Value1, Value2 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(source.EnumValue != null ? (global::D)source.EnumValue.Value : default(global::D?));
                return target;
                """
            );
    }

    [Fact]
    public void RecordToRecordNullableToNullableStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(C? StructValue);",
            "record B(D? StructValue);",
            "struct C { public int Value { get; set; } }",
            "struct D { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B(source.StructValue != null ? MapToD(source.StructValue.Value) : default(global::D?));
                return target;
                """
            );
    }

    [Fact]
    public void ClassConstructorParameterWithStringFormat()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "value", StringFormat = "C")]
            partial B Map(A source);",
            """,
            "class A { public int Value { get; set; } }",
            "class B { public B(string value) {} }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.Value.ToString("C", null));
                return target;
                """
            );
    }
}
