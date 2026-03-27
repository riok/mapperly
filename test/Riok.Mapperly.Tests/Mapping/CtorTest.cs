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

    [Fact]
    public void NullableSourceCtorCustomClassWithNullableCtorParam()
    {
        var source = TestSourceBuilder.Mapping("string?", "A", "class A { public A(string? x) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void NullableArraySourceCtorWithNullableArrayParam()
    {
        var source = TestSourceBuilder.Mapping("int[]?", "A", "record A(int[]? Value);");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::A(source);");
    }

    [Fact]
    public void NullableArraySourceCtorWithNonNullableArrayParamShouldThrow()
    {
        var source = TestSourceBuilder.Mapping("int[]?", "A", "class A { public A(int[] x) {} }");
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type int[]? to target of type A which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                "return source == null ? throw new global::System.ArgumentNullException(nameof(source)) : new global::A(source);"
            );
    }

    [Fact]
    public void NullableClassSourceCtorWithNullableClassParam()
    {
        var source = TestSourceBuilder.Mapping("A?", "B", "class A { }", "class B { public B(A? a) {} }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return new global::B(source);");
    }

    [Fact]
    public void DeepCloneClassWithCopyCtorAndMapperConstructorShouldUseAttributedCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(A.Value), Use = nameof(Transform))]
            public partial A Clone(A source);

            [UserMapping(Default = false)]
            private string Transform(string value) => $"transformed-{value}";
            """,
            TestSourceBuilderOptions.WithDeepCloning,
            """
            class A
            {
                [MapperConstructor]
                public A() { }
                public A(A other) { Value = other.Value; }
                public required string Value { get; init; }
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A()
                {
                    Value = Transform(source.Value),
                };
                return target;
                """
            );
    }

    [Fact]
    public void MapperConstructorShouldOverrideCopyCtorWithoutDeepCloning()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            """
            class A
            {
                public int Value { get; set; }
            }
            """,
            """
            class B
            {
                [MapperConstructor]
                public B() { }
                public B(A other) { Value = other.Value; }
                public int Value { get; set; }
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapPropertyShouldOverrideCopyCtorSelection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(A.Value), Use = nameof(Transform))]
            public partial A Clone(A source);

            [UserMapping(Default = false)]
            private string Transform(string value) => $"transformed-{value}";
            """,
            TestSourceBuilderOptions.WithDeepCloning,
            """
            class A
            {
                public A() { }
                public A(A other) { Value = other.Value; }
                public required string Value { get; init; }
            }
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A()
                {
                    Value = Transform(source.Value),
                };
                return target;
                """
            );
    }

    [Fact]
    public void NullableSourceMapperConstructorShouldOverrideCopyCtorSelection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "public partial B Map(A? source);",
            """
            class A
            {
                public int Value { get; set; }
            }
            """,
            """
            class B
            {
                [MapperConstructor]
                public B() { }
                public B(A value) { Value = value.Value; }
                public int Value { get; set; }
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    throw new global::System.ArgumentNullException(nameof(source));
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void NullableSourceMapPropertyShouldOverrideCopyCtorSelection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Value), Use = nameof(Transform))]
            public partial B Map(A? source);

            [UserMapping(Default = false)]
            private string Transform(string value) => $"transformed-{value}";
            """,
            """
            class A
            {
                public required string Value { get; init; }
            }
            """,
            """
            class B
            {
                public B() { }
                public B(A value) { Value = value.Value; }
                public required string Value { get; init; }
            }
            """
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    throw new global::System.ArgumentNullException(nameof(source));
                var target = new global::B()
                {
                    Value = Transform(source.Value),
                };
                return target;
                """
            );
    }
}
