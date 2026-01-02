using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ToObjectDeepCloningTest
{
    [Fact]
    public void ClassToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("A", "object", TestSourceBuilderOptions.WithDeepCloning, "class A {}");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return (object)MapToA(source);");
    }

    [Fact]
    public void ObjectToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("object", "object", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveMapMethodBody("return source;")
            .HaveDiagnostic(DiagnosticDescriptors.MappedObjectToObjectWithoutDeepClone)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void NullableObjectToNullableObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("object?", "object?", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowInfoDiagnostics)
            .Should()
            .HaveMapMethodBody("return source;")
            .HaveDiagnostic(DiagnosticDescriptors.MappedObjectToObjectWithoutDeepClone)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void StringToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("string", "object", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void BuiltInStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("DateTime", "object", TestSourceBuilderOptions.WithDeepCloning);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void CustomReadOnlyStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("A", "object", TestSourceBuilderOptions.WithDeepCloning, "readonly struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (object)source;");
    }

    [Fact]
    public void CustomMutableStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "object",
            TestSourceBuilderOptions.WithDeepCloning,
            "struct A { public string Value { get; set; } }"
        );
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return (object)MapToA(source);");
    }

    [Fact]
    public void CustomUnmanagedStructToObjectDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("A", "object", TestSourceBuilderOptions.WithDeepCloning, "struct A {}");
        TestHelper.GenerateMapper(source).Should().HaveMapMethodBody("return (object)source;");
    }
}
