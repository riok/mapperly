using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UserMethodAdditionalParametersTest
{
    [Fact]
    public Task AdditionalIntParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AdditionalNullableIntParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int? value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void AdditionalInitParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    Value = value.ToString(),
                };
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void AdditionalInitNullableIntParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int? value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    Value = value != null ? value.Value.ToString() : throw new System.ArgumentNullException(nameof(value.Value)),
                };
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public Task TwoAdditionalParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value, int id);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } public int Id { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void PreferParameterOverSourceMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member Value on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    Value = value,
                };
                return target;
                """
            );
    }

    [Fact]
    public void ClassFlattening()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, C nested);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string NestedValue { get; init; } }",
            "class C { public int Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    NestedValue = nested.Value.ToString(),
                };
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassFlattening()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, C? nested);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string NestedValue { get; init; } }",
            "class C { public int Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    NestedValue = nested != null ? nested.Value.ToString() : throw new System.ArgumentNullException(nameof(nested.Value)),
                };
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public void NullableClassToNullableFlattening()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, C? nested);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int? NestedValue { get; init; } }",
            "class C { public int Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    NestedValue = nested?.Value,
                };
                target.StringValue = src.StringValue;
                return target;
                """
            );
    }

    [Fact]
    public Task WithReferenceHandling()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithReferenceHandlingParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value, [ReferenceHandler] IReferenceHandler refHandler);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithReferenceHandlingAsFirstParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map([ReferenceHandler] IReferenceHandler refHandler, A src, int value);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExplicitDefaultShouldDiagnosticAndNotBeUsedAsDefault()
    {
        // D.Value => E.Value should not use the Map method
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping(Default = true)]
            partial B Map(A src, int value);
            partial E MapNested(D src);
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }",
            "class D { public A? Value { get; set; }}",
            "class E { public B? Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ShouldNotBeMarkedAsImplicitDefaultMapping()
    {
        // D.Value => E.Value should not use the Map method
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, int value);
            partial E MapNested(D src);
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; init; } }",
            "class D { public A? Value { get; set; }}",
            "class E { public B? Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void UnusedParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value2);",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.AdditionalParameterNotMapped,
                "The additional mapping method parameter value2 of the method Map is not mapped"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    Value = src.Value,
                };
                return target;
                """
            );
    }

    [Fact]
    public void UnusedSourceMemberSameNameAsParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A src, int value);",
            "class A { public int value { get; set; } }",
            "class B { public int value { get; init; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.SourceMemberNotMapped,
                "The member value on the mapping source type A is not mapped to any member on the mapping target type B"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                var target = new global::B()
                {
                    value = value,
                };
                return target;
                """
            );
    }

    [Fact]
    public Task ExistingTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A src, B target, int value);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
