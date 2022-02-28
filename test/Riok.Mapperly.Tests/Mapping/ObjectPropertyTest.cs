using Microsoft.CodeAnalysis;

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
    public void NullableIntToNonNullableIntProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int? Value { get; set; } }",
            "class B { public int Value { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.Value = source.Value.Value;
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableStringToNonNullableStringProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string? Value { get; set; } }",
            "class B { public string Value { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.Value = source.Value;
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableClassToNonNullableClassProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.Value = MapToD(source.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableStringToNullableStringProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string? Value { get; set; } }",
            "class B { public string? Value { get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value = source.Value;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NonNullableClassToNullableClassProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public D? Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value = MapToD(source.Value);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void DisabledNullableClassPropertyToNonNullableProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "#nullable disable\n class A { public C Value { get; set; } }\n#nullable enable",
            "class B { public D Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.Value = MapToD(source.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableClassPropertyToDisabledNullableProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "#nullable disable\n class B { public D Value { get; set; } }\n#nullable enable",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.Value = MapToD(source.Value);
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableClassToNonNullableClassPropertyThrow()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.Default with { ThrowOnPropertyMappingNullMismatch = true },
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }");

        TestHelper.GenerateMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.Value = MapToD(source.Value);
    }
    else
    {
        throw new System.ArgumentNullException(nameof(source.Value));
    }

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
    public void ShouldUseUserProvidedMappingWithDisabledNullability()
    {
        var mapperBody = @"
partial B Map(A source);
D UserImplementedMap(C source) => new D();";

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C {}",
            "class D {}");

        TestHelper.GenerateSingleMapperMethodBody(
                source,
                TestHelperOptions.Default with { NullableOption = NullableContextOptions.Disable })
            .Should()
            .Be(@"if (source == null)
        return default;
    var target = new B();
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

    [Fact]
    public void ManualFlattenedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"Value.Id\", \"MyValueId\")] partial B Map(A source);",
            "class A { public C Value { get; set; } }",
            "class B { public string MyValueId { get; set; } }",
            "class C { public string Id { get; set; }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.MyValueId = source.Value.Id;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void AutoFlattenedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.ValueId = source.Value.Id;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void AutoFlattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public string ValueId { get; set; } }",
            "class C { public string Id { get; set; }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value != null)
    {
        target.ValueId = source.Value.Id;
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ManualUnflattenedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C { public string Id { get; set; }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value.Id = source.MyValueId;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ManualUnflattenedPropertyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\"), MapProperty($\"MyValueId2\", \"Value.Id2\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } public string MyValueId2 { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public string Id { get; set; } public string Id2 { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value ??= new();
    target.Value.Id = source.MyValueId;
    target.Value.Id2 = source.MyValueId2;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task ManualUnflattenedPropertyNullablePathNoParameterlessCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManualUnflattenedPropertySourcePropertyNotFoundShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueIdXXX\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManualUnflattenedPropertyTargetPropertyPathWriteOnlyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.Id\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManualUnflattenedPropertyTargetPropertyNotFoundShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty($\"MyValueId\", \"Value.IdXXX\")] partial B Map(A source);",
            "class A { public string MyValueId { get; set; } }",
            "class B { public C? Value { get; set; } }",
            "class C { public C(string arg) {} public string Id { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ManualNestedPropertyNullablePath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapProperty(\"Value1.Value1.Id1\", \"Value2.Value2.Id2\")]" +
            "[MapProperty(\"Value1.Value1.Id10\", \"Value2.Value2.Id20\")]" +
            "[MapProperty(new[] { \"Value1\", \"Id100\" }, new[] { \"Value2\", \"Id200\" })]" +
            "partial B Map(A source);",
            "class A { public C? Value1 { get; set; } }",
            "class B { public E? Value2 { get; set; } }",
            "class C { public D? Value1 { get; set; } public string Id100 { get; set; } }",
            "class D { public string Id1 { get; set; } public string Id10 { get; set; } }",
            "class E { public F? Value2 { get; set; } public string Id200 { get; set; } }",
            "class F { public string Id2 { get; set; } public string Id20 { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    if (source.Value1 != null)
    {
        target.Value2 ??= new();
        target.Value2.Id200 = source.Value1.Id100;
        if (source.Value1?.Value1 != null)
        {
            target.Value2.Value2 ??= new();
            target.Value2.Value2.Id2 = source.Value1.Value1.Id1;
            target.Value2.Value2.Id20 = source.Value1.Value1.Id10;
        }
    }

    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContextInNestedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C Value { get; set;} }",
            "class B { public D Value { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }");

        return TestHelper.VerifyGenerator(source, TestHelperOptions.Default with { NullableOption = NullableContextOptions.Disable });
    }
}
