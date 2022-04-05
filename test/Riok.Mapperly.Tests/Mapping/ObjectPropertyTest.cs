namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectPropertyTest
{
    [Fact]
    public void OneSimpleProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void SameType()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "class A { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }

    [Fact]
    public void SameTypeDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "class A { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new A();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void CustomRefStructToSameCustomStruct()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            "ref struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be("return source;");
    }

    [Fact]
    public void CustomRefStructToSameCustomStructDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "ref struct A {}");
        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new A();
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void StringToIntProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string Value { get; set; } }",
            "class B { public int Value { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value = int.Parse(source.Value);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task ShouldIgnoreWriteOnlyPropertyOnSourceWithDiagnostics()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValue2 { set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldIgnoreReadOnlyPropertyOnTargetWithDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValue2 { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithUnmatchedPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValueA { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValueB { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithIgnoredProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(nameof(B.IntValue))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } }",
            "class B { public string StringValue { get; set; }  public int IntValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void WithManualMappedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue), nameof(B.StringValue2)] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue2 = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task WithManualMappedNotFoundTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue), nameof(B.StringValue9)] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithManualMappedNotFoundSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue9), nameof(B.StringValue2)] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ShouldUseUserImplementedMapping()
    {
        var mapperBody = @"
public partial B Map(A source);
private D UserImplementedMap(C source)
{
  var target = Map(source);
  target.StringValue += ""ok"";
  return target;
}
private partial D MapToD(C source);
";

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    target.NestedValue = UserImplementedMap(source.NestedValue);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task WithUnmappablePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public DateTime Value { get; set; } }",
            "class B { public Version Value { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithManualNotFoundTargetPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(nameof(A.StringValue), \"not_found\")] B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithManualNotFoundSourcePropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"not_found\", nameof(B.StringValue2))] partial B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithNotFoundIgnoredPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(\"not_found\")] partial B Map(A source);",
            "class A { }",
            "class B { }");

        return TestHelper.VerifyGenerator(source);
    }
}
