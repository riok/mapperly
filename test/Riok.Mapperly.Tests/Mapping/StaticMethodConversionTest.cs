using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class StaticMethodConversionTest
{
    [Fact]
    public void ClassWithStaticToTTargetMethod()
    {
        var source = TestSourceBuilder.Mapping("A", "byte", "class A { public static byte ToByte(A source) => 0; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToByte(source);");
    }

    [Fact]
    public void ClassWithStaticToTTargetArrayMethod()
    {
        var source = TestSourceBuilder.Mapping("A", "byte[]", "class A { public static byte[] ToByteArray(A source) => []; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToByteArray(source);");
    }

    [Fact]
    public void ClassWithStaticToTTargetMethodWhereTTargetHasKeyword()
    {
        var source = TestSourceBuilder.Mapping("A", "float", "class A { public static float ToFloat(A source) => 0; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToFloat(source);");
    }

    [Fact]
    public void ClassWithStaticToTTargetArrayMethodWhereTTargetHasKeyword()
    {
        var source = TestSourceBuilder.Mapping("A", "float[]", "class A { public static float[] ToFloatArray(A source) => []; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToFloatArray(source);");
    }

    [Fact]
    public void ClassWithStaticCreateMethod()
    {
        var source = TestSourceBuilder.Mapping("byte", "A", "class A { public static A Create(byte source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void ClassWithStaticCreateMethodForArraySource()
    {
        var source = TestSourceBuilder.Mapping("byte[]", "A", "class A { public static A Create(byte[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void ClassWithStaticCreateFromMethod()
    {
        var source = TestSourceBuilder.Mapping("byte", "A", "class A { public static A CreateFrom(byte source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void ClassWithStaticCreateFromMethodForArraySource()
    {
        var source = TestSourceBuilder.Mapping("byte[]", "A", "class A { public static A CreateFrom(byte[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void ClassWithStaticCreateFromArrayMethod()
    {
        var source = TestSourceBuilder.Mapping("byte[]", "A", "class A { public static A CreateFromArray(byte[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFromArray(source);");
    }

    [Fact]
    public void ClassWithStaticCreateFromTTargetArrayMethod()
    {
        var source = TestSourceBuilder.Mapping("byte[]", "A", "class A { public static A CreateFromByteArray(byte[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFromByteArray(source);");
    }

    [Fact]
    public void ClassWithStaticCreateFromTTargetMethod()
    {
        var source = TestSourceBuilder.Mapping("byte", "A", "class A { public static A CreateFromByte(byte source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFromByte(source);");
    }

    [Fact]
    public void ClassWithStaticCreateFromTTargetWhereTTargetIsKeywordMethod()
    {
        var source = TestSourceBuilder.Mapping("float", "A", "class A { public static A CreateFromFloat(float source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFromFloat(source);");
    }

    [Fact]
    public void ClassWithStaticFromTTargetMethod()
    {
        var source = TestSourceBuilder.Mapping("byte", "A", "class A { public static A FromByte(byte source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromByte(source);");
    }

    [Fact]
    public void ClassWithStaticFromTTargetArrayMethod()
    {
        var source = TestSourceBuilder.Mapping("byte[]", "A", "class A { public static A FromByteArray(byte[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromByteArray(source);");
    }

    [Fact]
    public void MethodNameCaseInsensitive()
    {
        var source = TestSourceBuilder.Mapping("byte[]", "A", "class A { public static A frombytearray(byte[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.frombytearray(source);");
    }

    [Fact(Skip = "RMG090: Mapping the nullable source of type B? to target of type A which is not nullable")]
    public void MapNullableReferenceTypeToNonNullableReferenceType()
    {
        var source = TestSourceBuilder.Mapping("B?", "A", "class A { public static A FromB(B? source) => new(); }; class B {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromB(source);");
    }

    [Fact]
    public void MapNonNullableReferenceTypeToNullableReferenceType()
    {
        var source = TestSourceBuilder.Mapping("B", "A?", "class A { public static A FromB(B source) => new(); }; class B {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromB(source);");
    }

    [Fact(Skip = "RMG090: Mapping the nullable source of type B? to target of type A which is not nullable")]
    public void MapNullableValueTypeToNonNullableValueType()
    {
        var source = TestSourceBuilder.Mapping("B?", "A", "struct A { public static A FromB(B? source) => new(); }; struct B {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromB(source);");
    }

    [Fact]
    public void MapNonNullableValueTypeToNullableValueType()
    {
        var source = TestSourceBuilder.Mapping("B", "A?", "struct A { public static A FromB(B source) => new(); }; struct B {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::A?)global::A.FromB(source);");
    }

    [Fact]
    public void MapNonNullableValueTypeToNullableValueTypeWithNullableMethodParameter()
    {
        var source = TestSourceBuilder.Mapping("B", "A?", "struct A { public static A FromB(B? source) => new(); }; struct B {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromB(source);");
    }

    [Fact]
    public void CustomTypeWithStaticToTargetGenericMethod()
    {
        var source = TestSourceBuilder.Mapping("A<byte>", "List<byte>", "class A<T> { public static List<T> ToList(A<T> source) => []; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A<byte>.ToList(source);");
    }

    [Fact]
    public void DateTimeToDateOnlyMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "DateOnly",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.DateTimeToDateOnly)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DateTimeToTimeOnlyMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "TimeOnly",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.DateTimeToTimeOnly)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void StaticMethodMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "int",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.StaticConvertMethods),
            "class A { public static int ToInt32() => 0; }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void IgnoredSourceStaticMethodShouldNotBeUsed()
    {
        var source = TestSourceBuilder.Mapping("A", "int", "class A { [MapperIgnore] public static int ToInt32(A source) => 42; }");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void IgnoredTargetFactoryMethodShouldBeSkippedIfAlternativeExists()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { [MapperIgnore] public static A"
                + nameof(FromB)
                + "(B source) => new(); public static A CreateFrom(B source) => new(); }",
            "class B {}"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void IgnoredTargetFactoryMethodShouldFallbackToConstructor()
    {
        var source = TestSourceBuilder.Mapping(
            "B",
            "A",
            "class A { public A(B source) {} [MapperIgnore] public static A FromB(B source) => new(source); }",
            "class B {}"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }
}
