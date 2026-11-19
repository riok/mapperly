using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class UserMethodMappingTargetOriginalValueTest
{
    [Fact]
    public Task BasicTargetOriginalValueParameter()
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
    public Task TargetOriginalValueParameterBeforeSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int? FromOptional([MappingTargetOriginalValue] int? original, Optional source)
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
    public Task TargetOriginalValueParameterWithNonNullableTarget()
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
    public Task GenericMethodWithTargetOriginalValueParameter()
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
    public Task GenericMethodWithTargetOriginalValueParameterForInitOnlyMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static T? FromOptional<T>(Optional<T> source, [MappingTargetOriginalValue] T? original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            "class A { public Optional<int?> Age { get; set; } }",
            "class B { public int? Age { get; init; } }",
            "struct Optional<T> { public bool HasValue { get; } public T Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MultiplePropertiesWithTargetOriginalValueParameter()
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
    public void TargetOriginalValueParameterOnGeneratedMethodShouldDiagnostic()
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
    public void TargetOriginalValueParameterOnRuntimeTargetGeneratedMethodShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial object Map(object src, [MappingTargetOriginalValue] System.Type targetType);
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
    public void TargetOriginalValueParameterOnExistingTargetUserImplementedMethodShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [UserMapping]
            private static void MapValue(Optional source, [MappingTarget] OptionalDto target, [MappingTargetOriginalValue] OptionalDto original)
            {
            }

            partial B Map(A src);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }",
            "class Optional { public int Value { get; set; } }",
            "class OptionalDto { public int Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature, "MapValue has an unsupported mapping method signature")
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void MultipleTargetOriginalValueParametersShouldError()
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
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotMapMember)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task TargetOriginalValueParameterForInitOnlyMember()
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

    [Fact]
    public Task TargetOriginalValueParameterWithPrivateTargetProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private static int? FromOptional(Optional source, [MappingTargetOriginalValue] int? original)
                => source.HasValue ? source.Value : original;
            partial B Map(A src);
            """,
            TestSourceBuilderOptions.WithMemberVisibility(MemberVisibility.All),
            "class A { private Optional Age { get; set; } }",
            "class B { private int? Age { get; set; } }",
            "struct Optional { public bool HasValue { get; } public int? Value { get; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NamedMappingForExistingTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            partial void Map(A src, B target);

            [UserMapping(Default = false)]
            private static int? ResolveCount(int? value, [MappingTargetOriginalValue] int? original) => value ?? original;
            """,
            "class A { public int? Value { get; set; } }",
            "class B { public int? Count { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NamedMappingForNewInstance()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            partial B Map(A src);

            [UserMapping(Default = false)]
            private static int? ResolveCount(int? value, [MappingTargetOriginalValue] int? original) => value ?? original;
            """,
            "class A { public int? Value { get; set; } }",
            "class B { public int? Count { get; set; } = 42; }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NamedMappingFromSourceWithSupertypeParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapPropertyFromSource(nameof(B.Count), Use = nameof(ResolveCount))]
            partial void Map(A src, B target);

            private static int? ResolveCount(ICount value, [MappingTargetOriginalValue] int? original) => value.GetCount() ?? original;
            """,
            "interface ICount { int? GetCount(); }",
            "class A : ICount { public int? GetCount() => null; }",
            "class B { public int? Count { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NamedMappingWithAdditionalParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            partial B Map(A src, int multiplier);

            private static int? ResolveCount(int? value, int multiplier, [MappingTargetOriginalValue] int? original)
                => value * multiplier ?? original;
            """,
            "class A { public int? Value { get; set; } }",
            "class B { public int? Count { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExternalStaticNamedMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(@Ext.ResolveCount))]
            partial void Map(A src, B target);
            """,
            "class A { public int? Value { get; set; } }",
            "class B { public int? Count { get; set; } }",
            """
            static class Ext
            {
                public static int? ResolveCount(int? value, [MappingTargetOriginalValue] int? original) => value ?? original;
            }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NonInlinedNamedMappingForProjection()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<B> Project(IQueryable<A> query);

            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            private static partial B Map(A src);

            [UserMapping(Default = false)]
            [MapperNoExpressionInlining]
            private static int? ResolveCount(int? value, [MappingTargetOriginalValue] int? original) => value ?? original;
            """,
            "public class A { public int? Value { get; set; } }",
            "public record B(int? Count);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void TargetOriginalValueParameterForNullableTargetPath()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), "Child.Name")]
            partial void Map(A src, B target);

            private static string FromOptional(Optional source, [MappingTargetOriginalValue] string original)
                => source.Value ?? original;
            """,
            "class A { public Optional Value { get; set; } = new(); }",
            "class B { public C? Child { get; set; } }",
            "class C { public string Name { get; set; } = \"\"; }",
            "class Optional { public string? Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                target.Child ??= new global::C();
                target.Child.Name = FromOptional(src.Value, target.Child.Name);
                """
            );
    }

    [Fact]
    public void NamedMappingForSetOnlyMemberShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            partial void Map(A src, B target);

            [UserMapping(Default = false)]
            private static int? ResolveCount(int? value, [MappingTargetOriginalValue] int? original) => value ?? original;
            """,
            "class A { public int? Value { get; set; } }",
            "class B { private int? _count; public int? Count { set => _count = value; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MappingTargetOriginalValueMemberNotReadable,
                "The target member B.Count of type int? cannot be read and cannot be passed to the [MappingTargetOriginalValue] parameter original in ResolveCount, the default value is used instead"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("target.Count = ResolveCount(src.Value, default(int?));");
    }

    [Fact]
    public void NamedMappingWithIncompatibleOriginalValueTypeShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            partial void Map(A src, B target);

            [UserMapping(Default = false)]
            private static int ResolveCount(string value, [MappingTargetOriginalValue] int original) => value.Length + original;
            """,
            "class A { public string Value { get; set; } = \"\"; }",
            "class B { public long Count { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferencedMappingTargetTypeMismatch,
                "The target type int of the referenced mapping ResolveCount does not match the expected type long"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.MappingTargetOriginalValueTypeMismatch,
                "The target member B.Count of type long cannot be passed to the [MappingTargetOriginalValue] parameter original of type int in ResolveCount, the default value is used instead"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("target.Count = (long)ResolveCount(src.Value, default(int));");
    }

    [Fact]
    public void IncompatibleOriginalValueTypeForInitOnlyMemberShouldNotDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial B Map(A src);

            private static long FromOptional(Optional source, [MappingTargetOriginalValue] int original)
                => source.HasValue ? source.Value : original;
            """,
            "class A { public Optional Count { get; set; } }",
            "class B { public long Count { get; init; } }",
            "struct Optional { public bool HasValue { get; } public long Value { get; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B()
                {
                    Count = FromOptional(src.Count, default(int)),
                };
                return target;
                """
            );
    }

    [Fact]
    public void NamedMappingShouldNotPassOriginalValueToSourceConversion()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(A.Value), nameof(B.Count), Use = nameof(ResolveCount))]
            partial void Map(A src, B target);

            [UserMapping(Default = false)]
            private static int ResolveCount(Wrapper value, [MappingTargetOriginalValue] int original) => value.Count + original;

            private static Wrapper ToWrapper(string value, [MappingTargetOriginalValue] Wrapper original) => original;
            """,
            "class A { public string Value { get; set; } = \"\"; }",
            "class B { public int Count { get; set; } }",
            "class Wrapper { public int Count { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.ReferencedMappingSourceTypeMismatch,
                "The source type Wrapper of the referenced mapping ResolveCount does not match the expected type string"
            )
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody("target.Count = ResolveCount(ToWrapper(src.Value, default(global::Wrapper)), target.Count);");
    }
}
