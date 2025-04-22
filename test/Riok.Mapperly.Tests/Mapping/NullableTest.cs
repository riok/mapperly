using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class NullableTest
{
    [Fact]
    public void NullableToNonNullableShouldThrow()
    {
        var source = TestSourceBuilder.Mapping("A?", "B", "class A { }", "class B { }");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type A? to target of type B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    throw new global::System.ArgumentNullException(nameof(source));
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void NullableToNullableShouldWork()
    {
        var source = TestSourceBuilder.Mapping("A?", "B?", "class A { }", "class B { }");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return default;
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void NullablePrimitiveToOtherNullablePrimitiveShouldWork()
    {
        var source = TestSourceBuilder.Mapping("decimal?", "int?");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source == null ? default(int?) : (int)source.Value;");
    }

    [Fact]
    public void NonNullableToNullableShouldWork()
    {
        var source = TestSourceBuilder.Mapping("A", "B?", "class A { }", "class B { }");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void NullableToNonNullableWithNoThrowShouldReturnNewInstance()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "B",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnMappingNullMismatch = false,
            },
            "class A { }",
            "class B { }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type A? to target of type B which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return new global::B();
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public Task NullableToNonNullableWithNoThrowNoAccessibleCtorShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "string?",
            "B",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnMappingNullMismatch = false,
            },
            "class B { protected B(){} public static B Parse(string v) => new B(); }"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void NullableToNonNullableStringWithNoThrowShouldReturnEmptyString()
    {
        var source = TestSourceBuilder.Mapping(
            "A?",
            "string",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnMappingNullMismatch = false,
            },
            "class A { }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type A? to target of type string which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("return source == null ? \"\" : source.ToString();");
    }

    [Fact]
    public void NullableToNonNullableValueTypeWithNoThrowShouldReturnDefault()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime?",
            "DateTime",
            TestSourceBuilderOptions.Default with
            {
                ThrowOnMappingNullMismatch = false,
            }
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.NullableSourceTypeToNonNullableTargetType,
                "Mapping the nullable source of type System.DateTime? to target of type System.DateTime which is not nullable"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("return source == null ? default : source.Value;");
    }

    [Fact]
    public void NonNullableToNullableValueType()
    {
        var source = TestSourceBuilder.Mapping("DateTime", "DateTime?");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return (global::System.DateTime?)source;");
    }

    [Fact]
    public void WithExistingInstanceNullableSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A? source, B target)",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (source == null)
                    return;
                target.StringValue = source.StringValue;
                """
            );
    }

    [Fact]
    public void ToTargetShouldNotGenerateEmptyIfStatement()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial B Map(A source)",
            "class A { public C Value { get; set; } }",
            "class B { public D? Value { get; } }",
            "class C { public int Id { get; set; } }",
            "class D { public int Id { get;  } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                return target;
                """
            );
    }

    [Fact]
    public void ExistingTargetShouldNotGenerateEmptyIfStatement()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(A source, B destination)",
            "class A { public C Value { get; set; } }",
            "class B { public D? Value { get; } }",
            "class C { public int Id { get; set; } }",
            "class D { public int Id { get;  } }"
        );

        TestHelper.GenerateMapper(source, TestHelperOptions.AllowDiagnostics).Should().HaveSingleMethodBody("");
    }

    [Fact]
    public Task ShouldUpgradeNullabilityInDisabledNullableContext()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ShouldUpgradeArrayElementNullabilityInDisabledNullableContext()
    {
        var source = TestSourceBuilder.Mapping(
            "A[]",
            "B[]",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ShouldUpgradeGenericNullabilityInDisabledNullableContext()
    {
        var source = TestSourceBuilder.Mapping(
            "IEnumerable<A>",
            "IReadOnlyCollection<B>",
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );
        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }
}
