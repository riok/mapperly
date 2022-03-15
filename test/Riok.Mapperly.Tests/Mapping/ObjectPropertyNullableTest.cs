using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectPropertyNullableTest
{
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
