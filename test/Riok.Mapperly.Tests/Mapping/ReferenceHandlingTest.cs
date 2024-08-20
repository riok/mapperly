using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ReferenceHandlingTest
{
    [Fact]
    public Task ShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ManuallyMappedPropertiesShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Value", "MyValue")] private partial B MapToB(A source);
            [MapProperty("Value", "MyValue2")] private partial B MapToB1(A source);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D MyValue { get; set; } public D MyValue2 { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task EnumerableShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public IEnumerable<A> Parent { get; set; } public C Value { get; set; } }",
            "class B { public IEnumerable<B> Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ArrayShouldWork()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A[] Parent { get; set; } public C Value { get; set; } }",
            "class B { public B[] Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExistingInstanceShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map(A source, B target);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ObjectFactoryShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [ObjectFactory] B CreateB() => new B();
            private partial B Map(A a);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task RuntimeTargetTypeShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial object Map(object source, Type destinationType);

            private partial B MapToB(A source, [ReferenceHandler] IReferenceHandler refHandler);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public int IntValue { get; set; } }",
            "class B { public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CustomHandlerShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void CustomHandlerWithWrongTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source, [ReferenceHandler] MyRefHandler refHandler);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class MyRefHandler : IReferenceHandler {}",
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferenceHandlerParameterWrongType,
                "The reference handler parameter of Mapper.Map needs to be of type Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler but is MyRefHandler"
            );
    }

    [Fact]
    public Task RuntimeTargetTypeWithReferenceHandlingShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial object Map(object source, Type destinationType, [ReferenceHandler] IReferenceHandler refHandler);

            private partial B MapToB(A source, [ReferenceHandler] IReferenceHandler refHandler);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public int IntValue { get; set; } }",
            "class B { public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void CustomHandlerWithDisabledReferenceHandlingShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler);",
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferenceHandlingNotEnabled,
                "Mapper.Map uses reference handling, but it is not enabled on the mapper attribute, to enable reference handling set UseReferenceHandling to true"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task CustomHandlerWithObjectFactoryShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "[ObjectFactory] B CreateB() => new B();" + "private partial B Map(A a, [ReferenceHandler] IReferenceHandler refHandler);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CustomHandlerWithExistingInstanceShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map(A a, B b, [ReferenceHandler] IReferenceHandler refHandler);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CustomHandlerAtIndex0ShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial B Map([ReferenceHandler] IReferenceHandler refHandler, A a);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CustomHandlerAtIndex1WithExistingInstanceShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial void Map([ReferenceHandler] IReferenceHandler refHandler, A a, B b);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedWithoutReferenceHandlerShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """private partial B Map(A a); string ToStringMod(string s) => s + "-modified";""",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedWithReferenceHandlerShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """private partial B Map(A a); string ToStringMod(string s, [ReferenceHandler] IReferenceHandler _) => s + "-modified";""",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public A Parent { get; set; } public C Value { get; set; } }",
            "class B { public B Parent { get; set; } public D Value { get; set; } }",
            "class C { public string StringValue { get; set; } }",
            "class D { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MultipleUserDefinedWithSpecifiedDefault()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial B Map(A a);
            private partial D MapD(C source);
            [MapperIgnoreSource("IntValue")]
            [MapperIgnoreTarget("IntValue")]
            [UserMapping(Default = true)]
            private partial D MapDIgnore(C source);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "record A(C Value);",
            "record B(D Value);",
            "record C(string StringValue, int IntValue);",
            "record D(string StringValue) { public int IntValue { get; set; } };"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void ReferenceHandlerParameterIsAlsoMappingTargetParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "public partial B Map(A source, [MappingTarget, ReferenceHandler] IReferenceHandler refHandler);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "record A;",
            "record B;"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "Map has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DuplicatedReferenceHandlerParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "public partial B Map(A source, [ReferenceHandler] IReferenceHandler refHandler, [ReferenceHandler] IReferenceHandler refHandler1);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "record A;",
            "record b;"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "Map has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }
}
