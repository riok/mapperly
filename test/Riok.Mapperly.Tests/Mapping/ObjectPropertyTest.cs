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
    target.Value = source.Value ?? default;
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
    target.Value = source.Value ?? """";
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableToNonNullableWithUserImplementedMethodAsAbstractClass()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper]
public abstract class MyMapper
{
    public abstract B Map(A source);

    public string? MapString(string s)
        => s;
}

class A { public string Value { get; set; } }
class B { public string Value { get; set; } }
";

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value = MapString(source.Value) ?? """";
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableToNonNullableWithUserImplementedMethodAsInterface()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

[Mapper]
public interface IMyMapper
{
    B Map(A source);

    string? MapString(string s) => s;
}

class A { public string Value { get; set; } }
class B { public string Value { get; set; } }
";

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.Value = ((IMyMapper)this).MapString(source.Value) ?? """";
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public Task NullableClassToNonNullableClassProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public C? Value { get; set; } }",
            "class B { public D Value { get; set; } }",
            "class C { public string V {get; set; } }",
            "class D { public string V {get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ShouldIgnoreWriteOnlyPropertyOnSource()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValue2 { set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldIgnoreReadOnlyPropertyOnTarget()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValue2 { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValue2 { get; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void WithUnmatchedProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public string StringValue { get; set; } public string StringValueA { get; set; } }",
            "class B { public string StringValue { get; set; } public string StringValueB { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void WithIgnoredProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(nameof(B.IntValue))] B Map(A source);",
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
            "[MapProperty(nameof(A.StringValue), nameof(B.StringValue2)] B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue2 = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseUserProvidedMappingAsInterface()
    {
        var mapperBody = @"
B Map(A source);
D UserImplementedMap(C source) => new D();";

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C {}",
            "class D {}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    target.NestedValue = ((IMapper)this).UserImplementedMap(source.NestedValue);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseUserProvidedMappingAsAbstractClass()
    {
        var mapperBody = @"
public abstract B Map(A source);
public D UserImplementedMap(C source) => new D();";

        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            mapperBody,
            TestSourceBuilderOptions.Default with { AsInterface = false },
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C {}",
            "class D {}");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    target.NestedValue = UserImplementedMap(source.NestedValue);
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableToNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "B?",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"if (source == null)
        return default;
    var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void NullableToNonNullable()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "B",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"if (source == null)
        return new B();
    var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void CustomClassToNullableCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B?",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateSingleMapperMethodBody(source)
            .Should()
            .Be(@"var target = new B();
    target.StringValue = source.StringValue;
    return target;".ReplaceLineEndings());
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
            "[MapProperty(\"not_found\", nameof(B.StringValue2))] B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithNotFoundIgnoredPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[MapperIgnore(\"not_found\")] B Map(A source);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } }");

        return TestHelper.VerifyGenerator(source);
    }
}
