using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ObjectFactoryTest
{
    [Fact]
    public void ShouldUseSimpleObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new B();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseGenericObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<T>() where T : new() => new T();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = Create<B>();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseFirstMatchingObjectFactory()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] A CreateA() => new A();"
            + "[ObjectFactory] B CreateB() => new B();"
            + "[ObjectFactory] T Create<T>() where T : new() => new T();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseGenericObjectFactoryWithTypeConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] private T CreateA<T>() where T : new(), A => new T();"
            + "[ObjectFactory] private T CreateStruct<T>() where T : new(), struct => new T();"
            + "[ObjectFactory] private T CreateB<T>() where T : new(), B => new T();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB<B>();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryIfTypeIsNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new B();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "#nullable disable\nclass B { public string StringValue { get; set; } }\n#nullable restore");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryAndCreateObjectIfNullIsReturned()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B? CreateB() => null;"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB() ?? new B();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseSimpleObjectFactoryAndThrowIfNoParameterlessCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B? CreateB() => null;"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { private B() {} public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = CreateB() ?? throw new System.NullReferenceException(""The object factory CreateB returned null"");
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseGenericObjectFactoryAndCreateObjectIfNullIsReturned()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T? Create<T>() where T : notnull => null;"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = Create<B>() ?? new B();
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void ShouldUseGenericObjectFactoryAndThrowIfNoParameterlessCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T? Create<T>() where T : notnull => null;"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { private B() {} public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(@"var target = Create<B>() ?? throw new System.NullReferenceException(""The object factory Create returned null"");
    target.StringValue = a.StringValue;
    return target;".ReplaceLineEndings());
    }

    [Fact]
    public void InvalidSignatureAsync()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] async Task<T> Create<T>() where T : new() => Task.FromResult(new T());"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.InvalidObjectFactorySignature));
    }

    [Fact]
    public void InvalidSignatureParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB(int i) => new B();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.InvalidObjectFactorySignature));
    }

    [Fact]
    public void InvalidSignatureVoid()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] void CreateB() => 0"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.InvalidObjectFactorySignature));
    }

    [Fact]
    public void InvalidSignatureMultipleTypeParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] T Create<T, T2>() where T : new() => new T();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.InvalidObjectFactorySignature));
    }

    [Fact]
    public void InvalidSignatureTypeParameterNotReturnType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB<T>() => new B();"
            + "partial B Map(A a);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }");

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(new(DiagnosticDescriptors.InvalidObjectFactorySignature));
    }
}
