using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UserMethodMappingTargetOriginalValueTest
{
    [Fact]
    public Task BasicDestinationValueParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int? FromOptional(Optional source, [MappingTargetOriginalValue] int? original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            "class A { public Optional Age { get; set; } }",
            "class B { public int? Age { get; set; } }",
            "struct Optional { public bool HasValue { get; } public int? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task DestinationValueParameterWithNonNullableTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int FromOptional(Optional source, [MappingTargetOriginalValue] int original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            "class A { public Optional Age { get; set; } }",
            "class B { public int Age { get; set; } }",
            "struct Optional { public bool HasValue { get; } public int Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task GenericMethodWithDestinationValueParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static T? FromOptional<T>(Optional<T> source, [MappingTargetOriginalValue] T? original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            "class A { public Optional<int?> Age { get; set; } }",
            "class B { public int? Age { get; set; } }",
            "struct Optional<T> { public bool HasValue { get; } public T Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MultiplePropertiesWithDestinationValueParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int? FromOptional(Optional source, [MappingTargetOriginalValue] int? original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            "class A { public Optional Age { get; set; } public Optional Count { get; set; } }",
            "class B { public int? Age { get; set; } public int? Count { get; set; } }",
            "struct Optional { public bool HasValue { get; } public int? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void DestinationValueParameterOnGeneratedMethodShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src, [MappingTargetOriginalValue] B original);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MappingOriginalValueNotSupportedForGeneratedMethod,
                "The [MappingTargetOriginalValue] attribute cannot be used on a generated (partial) mapping method parameter in Map"
            )
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MultipleDestinationValueParametersShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int? FromOptional(Optional source, [MappingTargetOriginalValue] int? dest1, [MappingTargetOriginalValue] int? dest2)
                => source.HasValue ? source.Value : dest1 ?? dest2;
            partial B Map(A src);
            """,
            "class A { public Optional Age { get; set; } }",
            "class B { public int? Age { get; set; } }",
            "struct Optional { public bool HasValue { get; } public int? Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MultipleOriginalValueParameters,
                "The mapping method FromOptional has multiple [MappingTargetOriginalValue] parameters, only one is allowed"
            )
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotMapMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task DestinationValueParameterForInitOnlyMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int? FromOptional(Optional source, [MappingTargetOriginalValue] int? original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            "class A { public Optional Age { get; set; } }",
            "class B { public int? Age { get; init; } }",
            "struct Optional { public bool HasValue { get; } public int? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
