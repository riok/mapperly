using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class CtorTest
{
    [Fact]
    public void CtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "class A { public A(string x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorCustomClassNullableParameter()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "class A { public A(string? x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorCustomClassNullablePrimitiveParameter()
    {
        var source = TestSourceBuilder.Mapping("int", "A", "class A { public A(int? x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorCustomStruct()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "struct A { public A(string x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void PrimaryCtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping("string", "A", "class A(string x) {}");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void CtorMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "string",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.ToStringMethod),
            "class A { public A(string x) {} }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.CouldNotCreateMapping,
                "Could not create mapping from A to string. Consider implementing the mapping manually."
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DeepCloneRecordShouldNotUseCtorMapping()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithDeepCloning,
            "record A { public int Value { get; set; } }"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapPropertyCtorMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.BId), nameof(B.Id))]
            private partial B Map(A source);
            """,
            "class A { public long BId { get; } }",
            """
            class B
            {
                public long Id { get; }

                public B(long id) { Id = id; }
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B(source.BId);
                return target;
                """
            );
    }

    [Fact]
    public Task PrivateCtorCustomClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A",
            TestSourceBuilderOptions.WithConstructorVisibility(MemberVisibility.All),
            "class A { private A(string x) {} }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateCtorCustomGenericClass()
    {
        var source = TestSourceBuilder.Mapping(
            "string",
            "A<Guid>",
            TestSourceBuilderOptions.WithConstructorVisibility(MemberVisibility.All),
            "class A<T> where T : struct { private A(string x) {} }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task PrivateCtorCustomClassWithCustomClassParam()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithConstructorVisibility(MemberVisibility.All),
            "class A;",
            "class B { private B(A source) {} }"
        );
        return TestHelper.VerifyGenerator(source);
    }
}
