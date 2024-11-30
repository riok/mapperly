using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class StaticMethodConversionTest
{
    #region CreateMethod

    [Fact]
    public void CustomClassWithStaticCreateMethod()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A Create(int source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateMethod()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "struct A { public static A Create(int source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateMethodWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A Create(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateMethodWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "struct A { public static A Create(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateMethodWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "class A { public static A Create(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateMethodWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "struct A { public static A Create(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateMethodWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A Create(params IList<int> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateMethodWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "struct A { public static A Create(params IEnumerable<int> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateMethodWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "class A { public static A Create(params IList<int> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateMethodWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "A",
            "struct A { public static A Create(params IEnumerable<int> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.Create(source);");
    }

    #endregion

    #region CreateFromMethod

    [Fact]
    public void CustomClassWithStaticCreateFromMethod()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A CreateFrom(int source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromMethod()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "struct A { public static A CreateFrom(int source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromMethodWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A CreateFrom(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromMethodWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "struct A { public static A CreateFrom(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromMethodWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "class A { public static A CreateFrom(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromMethodWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "struct A { public static A CreateFrom(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromMethodWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A CreateFrom(params IList<int> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromMethodWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping(
            "int",
            "A",
            "struct A { public static A CreateFrom(params IEnumerable<int> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromMethodWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "class A { public static A CreateFrom(params IList<int> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromMethodWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "A",
            "struct A { public static A CreateFrom(params IEnumerable<int> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.CreateFrom(source);");
    }

    #endregion

    #region FromTSourceMethod

    [Fact]
    public void CustomClassWithStaticFromSourceMethod()
    {
        var source = TestSourceBuilder.Mapping("DateTime", "A", "class A { public static A FromDateTime(DateTime source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromDateTime(source);");
    }

    [Fact]
    public void CustomStructWithStaticFromSourceMethod()
    {
        var source = TestSourceBuilder.Mapping("DateTime", "A", "struct A { public static A FromDateTime(DateTime source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromDateTime(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromSourceWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A FromInt32(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromSourceWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "struct A { public static A FromInt32(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromSourceWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "class A { public static A FromInt32Array(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32Array(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromSourceWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("int[]", "A", "struct A { public static A FromInt32Array(params int[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32Array(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromSourceWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public static A FromInt32(params IList<int> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromSourceWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping(
            "int",
            "A",
            "struct A { public static A FromInt32(params IEnumerable<int> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32(source);");
    }

    [Fact]
    public void CustomClassWithStaticCreateFromSourceWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "A",
            "class A { public static A FromInt32Array(params IList<int> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32Array(source);");
    }

    [Fact]
    public void CustomStructWithStaticCreateFromSourceWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping(
            "int[]",
            "A",
            "struct A { public static A FromInt32Array(params IEnumerable<int> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.FromInt32Array(source);");
    }

    #endregion

    #region ToTTargetMethod

    [Fact]
    public void CustomClassWithStaticToTargetMethod()
    {
        var source = TestSourceBuilder.Mapping("A", "DateTime", "class A { public static DateTime ToDateTime(A source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToDateTime(source);");
    }

    [Fact]
    public void CustomStructWithStaticToTargetMethod()
    {
        var source = TestSourceBuilder.Mapping("A", "DateTime", "struct A { public static DateTime ToDateTime(A source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToDateTime(source);");
    }

    [Fact]
    public void CustomClassWithStaticToTargetWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("A", "int[]", "class A { public static int[] ToInt32Array(params A[] source) => []; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32Array(source);");
    }

    [Fact]
    public void CustomStructWithStaticToTargetWithParamsArrayArgument1()
    {
        var source = TestSourceBuilder.Mapping("A", "int", "struct A { public static int ToInt32(params A[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32(source);");
    }

    [Fact]
    public void CustomClassWithStaticToTargetWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("A[]", "int[]", "class A { public static int[] ToInt32Array(params A[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32Array(source);");
    }

    [Fact]
    public void CustomStructWithStaticToTargetWithParamsArrayArgument2()
    {
        var source = TestSourceBuilder.Mapping("A[]", "int", "struct A { public static int ToInt32(params A[] source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32(source);");
    }

    [Fact]
    public void CustomClassWithStaticToTargetWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping("A", "int", "class A { public static int ToInt32(params IList<A> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32(source);");
    }

    [Fact]
    public void CustomStructWithStaticToTargetWithParamsCollectionArgument1()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "int",
            "struct A { public static int ToInt32(params IEnumerable<A> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32(source);");
    }

    [Fact]
    public void CustomClassWithStaticToTargetWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping("A[]", "int", "class A { public static int ToInt32(params IList<A> source) => new(); }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32(source);");
    }

    [Fact]
    public void CustomStructWithStaticToTargetWithParamsCollectionArgument2()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "int[]",
            "struct A { public static int[] ToInt32Array(params IEnumerable<A> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32Array(source);");
    }

    [Fact]
    public void CustomClassWithStaticToTargetWithParamsCollectionArgument3()
    {
        var source = TestSourceBuilder.Mapping(
            "IList<A>",
            "int",
            "class A { public static int ToInt32(params IList<A> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32(source);");
    }

    [Fact]
    public void CustomStructWithStaticToTargetWithParamsCollectionArgument3()
    {
        var source = TestSourceBuilder.Mapping(
            "IList<A>",
            "int[]",
            "struct A { public static int[] ToInt32Array(params IEnumerable<A> source) => new(); }"
        );
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A.ToInt32Array(source);");
    }

    #endregion

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
}
