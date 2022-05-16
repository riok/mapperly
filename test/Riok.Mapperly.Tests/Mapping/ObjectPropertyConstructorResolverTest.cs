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
            "class B { public B(string stringValue) {} { public int IntValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue);
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task ClassToClassWithOneMatchingCtorAndUnmatchedSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue) {} { }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ClassToClassWithOneMatchingCtorWithMatchedOptional()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue, int intValue = 10) {} }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue, source.IntValue);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ClassToClassWithOneMatchingCtorWithUnmatchedOptional()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string stringValue, int intValue2 = 10) {} public int IntValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue);
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ClassToClassWithOneMatchingCtorWithMatchedAndUnmatchedOptionals()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } public int IntValue3 { get; set; } }",
            "class B { public B(string stringValue, int intValue2 = 10, int intValue = 20) {} public int IntValue3 { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue, intValue: source.IntValue);
    target.IntValue3 = source.IntValue3;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldChooseCorrect()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string x) { } public B(string stringValue, int intvalue) { } public B(string stringValue) { } { public int IntValue { get; set; } ");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue, source.IntValue);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task ClassToClassPrivateCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { private B(){} }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUnmatchedCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string StringValue9){} public int IntValue { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUnmatchedAttributedCtorShouldDiagnosticAndUseAlternative()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { [MapperConstructor] public B(string StringValue9){} public B(string StringValue) {} public int IntValue { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldUseAttributed()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string x) { } [MapperConstructor] public B(string stringValue) { } public B() { } { public string StringValue { get; set; } public int IntValue { get; set; } ");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue);
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ClassToClassMultipleCtorsShouldPreferParameterless()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public B(string x) { } public B(string stringValue, int intvalue) { } public B() { } { public string StringValue { get; set; } public int IntValue { get; set; } ");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToRecord()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(string StringValue) { public int IntValue { get; set; } }",
            "record B(string StringValue) { public int IntValue { get; set; } ");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue);
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ClassToRecord()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "record B(string StringValue) { public int IntValue { get; set; } ");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.StringValue);
    target.IntValue = source.IntValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToFlattenedRecord()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(C Nested);",
            "record B(string NestedValue);",
            "record C(string Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.Nested.Value);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToFlattenedRecordNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(C? Nested);",
            "record B(string NestedValue);",
            "record C(string Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.Nested?.Value ?? throw new System.ArgumentNullException(nameof(source.Nested?.Value)));
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToFlattenedRecordNullablePathWithCustomMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source);" +
            "string StrConversion(string? s) => s ?? \"\";",
            "record A(C? Nested);",
            "record B(string NestedValue);",
            "record C(string Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(StrConversion(source.Nested?.Value));
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToFlattenedRecordNullablePathNoThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with { ThrowOnMappingNullMismatch = false },
            "record A(C? Nested);",
            "record B(string NestedValue);",
            "record C(string Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.Nested?.Value ?? """");
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToRecordNonNullableToNullablePrimitive()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(int Value);",
            "record B(int? Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B((int? )source.Value);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToRecordNullableToNonNullablePrimitive()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(int? Value);",
            "record B(int Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.Value ?? throw new System.ArgumentNullException(nameof(source.Value)));
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToRecordNonDirectAssignmentNullHandling()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(int? Value);",
            "record B(double Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.Value == null ? throw new System.ArgumentNullException(nameof(source.Value.Value)) : (double)source.Value.Value);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void RecordToRecordFlattenedNonDirectAssignmentNullHandling()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "record A(C? Nested);",
            "record B(double NestedValue);",
            "record C(int Value);");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B(source.Nested == null ? throw new System.ArgumentNullException(nameof(source.Nested.Value)) : (double)source.Nested.Value);
    return target;".ReplaceLineEndings());
    }
}
