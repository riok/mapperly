using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ObjectPropertyValueMethodTest
{
    [Fact]
    public void MethodToProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("GuidValue", Use = nameof(NewGuid))] partial B Map(A source);
            Guid NewGuid() => Guid.NewGuid();
            """,
            "class A;",
            "class B { public Guid GuidValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.GuidValue = NewGuid();
                return target;
                """
            );
    }

    [Fact]
    public void MethodToNestedProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Nested.Value", Use = nameof(NewGuid))] partial B Map(A source);
            Guid NewGuid() => Guid.NewGuid();
            """,
            "class A;",
            "class B { public C Nested { get; set; } }",
            "class C { public Guid Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Nested.Value = NewGuid();
                return target;
                """
            );
    }

    [Fact]
    public void MethodToPrivateProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("GuidValue", Use = nameof(NewGuid))] partial B Map(A source);
            Guid NewGuid() => Guid.NewGuid();
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.Private),
            "class A;",
            "class B { private Guid GuidValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.SetGuidValue(NewGuid());
                return target;
                """
            );
    }

    [Fact]
    public void MethodToNestedPrivateProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Nested.Value", Use = nameof(NewGuid))] partial B Map(A source);
            Guid NewGuid() => Guid.NewGuid();
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A;",
            "class B { private C Nested { get; set; } }",
            "class C { public Guid Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                target.GetNested().Value = NewGuid();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnTypeMismatchShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("GuidValue", Use = nameof(NewId))] partial B Map(A source);
            int NewId() => 1;
            """,
            "class A;",
            "class B { public Guid GuidValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueMethodTypeMismatch,
                "Cannot assign method return type int of NewId() to B.GuidValue of type System.Guid"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnTypeNullableToNonNullableShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(BuildC))] partial B Map(A source);
            C? BuildC() => new C();
            """,
            "class A;",
            "class B { public C Value { get; set; } }",
            "class C;"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueMethodTypeMismatch,
                "Cannot assign method return type C? of BuildC() to B.Value of type C"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnTypeNonNullableToNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(BuildC))] partial B Map(A source);
            C BuildC() => new C();
            """,
            "class A;",
            "class B { public C? Value { get; set; } }",
            "class C;"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = BuildC();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnValueTypeNullableToNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(BuildC))] partial B Map(A source);
            C? BuildC() => C.C1;
            """,
            "class A;",
            "class B { public C? Value { get; set; } }",
            "enum C { C1 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = BuildC();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnValueTypeNonNullableToNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(BuildC))] partial B Map(A source);
            C BuildC() => C.C1;
            """,
            "class A;",
            "class B { public C? Value { get; set; } }",
            "enum C { C1 }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = BuildC();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnValueTypeNullableToNonNullableShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(BuildC))] partial B Map(A source);
            System.Nullable<C> BuildC() => C.C1;
            """,
            "class A;",
            "class B { public C Value { get; set; } }",
            "enum C { C1 }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAndIncludeAllDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueMethodTypeMismatch,
                "Cannot assign method return type C? of BuildC() to B.Value of type C"
            )
            .HaveDiagnostic(DiagnosticDescriptors.NoMemberMappings, "No members are mapped in the object mapping from A to B")
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void MethodReturnTypeInDisabledNullableContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", Use = nameof(BuildC))] partial B Map(A source);
            #nullable disable
            C BuildC() => new C();
            #nullable enable
            """,
            "class A;",
            "class B { public C Value { get; set; } }",
            "class C;"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = BuildC();
                return target;
                """
            );
    }

    [Fact]
    public void ValueAndMethodShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("Value", 10, Use = nameof(NewValue))] partial B Map(A source);
            int NewValue() => 11;
            """,
            "class A;",
            "class B { private int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.InvalidMapValueAttributeUsage, "Invalid usage of the MapValueAttribute")
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void UnknownMethodShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("GuidValue", Use = nameof(NewGuid))] partial B Map(A source);
            """,
            "class A;",
            "class B { public Guid GuidValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueReferencedMethodNotFound,
                "The referenced method NewGuid could not be found or has an unsupported signature"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void InvalidMethodSignatureAsyncShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapValue("GuidValue", Use = nameof(NewGuid))] partial B Map(A source);
            async Task<Guid> NewGuid() => await Task.FromResult(Guid.NewGuid());
            """,
            "class A;",
            "class B { public Guid GuidValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapValueReferencedMethodNotFound,
                "The referenced method NewGuid could not be found or has an unsupported signature"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }
}
