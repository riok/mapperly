using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class RuntimeTargetTypeMappingTest
{
    [Fact]
    public Task WithNullableObjectSourceAndTargetTypeShouldIncludeNullables()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial object? Map(object? source, Type targetType);

            private partial B MapToB(A source);
            private partial D? MapToD(C? source);
            private partial int? MapStringToInt(string? source);
            private partial int? MapIntToInt(int source);
            """,
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }",
            "class C { public string Value2 { get; set; } }",
            "class D { public string Value2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithNonNullableReturnTypeShouldOnlyIncludeNonNullableMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial object Map(object source, Type targetType);

            private partial B MapToB(A source);
            private partial D? MapToD(C source);
            private partial int? MapToInt(string? source);
            """,
            "class A {}",
            "class B {}",
            "class C {}",
            "class D {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithSubsetSourceTypeAndObjectTargetTypeShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial Base1Target Map(Base1Source source, Type targetType);

            private partial B MapToB(A source);
            private partial D MapToD(C source);
            """,
            "class Base1Source {}",
            "class Base2Source {}",
            "class Base1Target {}",
            "class Base2Target {}",
            "class A : Base1Source {}",
            "class B : Base1Target {}",
            "class C : Base2Source {}",
            "class D : Base2Target {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithTypeHierarchyShouldPreferMostSpecificMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial object Map(object source, Type targetType);

            private partial C MapAToC(A source);
            private partial C MapBToC(B source);
            private partial C MapB1ToC(Base1 source);
            private partial C MapB2ToC(Base2 source);
            """,
            "class Base1 {}",
            "class Base2 : Base1 {}",
            "class A : Base2 {}",
            "class B : Base1 {}",
            "class C {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when targetType.IsAssignableFrom(typeof(global::C)) => MapAToC(x),
                    global::B x when targetType.IsAssignableFrom(typeof(global::C)) => MapBToC(x),
                    global::Base2 x when targetType.IsAssignableFrom(typeof(global::C)) => MapB2ToC(x),
                    global::Base1 x when targetType.IsAssignableFrom(typeof(global::C)) => MapB1ToC(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithDerivedTypesShouldUseBaseType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial object Map(object source, Type targetType);

            [MapDerivedType<A, B>]
            [MapDerivedType<C, D>]
            partial BaseDto MapDerivedTypes(Base source);
            """,
            "class Base {}",
            "class BaseDto {}",
            "class A : Base {}",
            "class B : BaseDto {}",
            "class C : Base {}",
            "class D : BaseDto {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::Base x when targetType.IsAssignableFrom(typeof(global::BaseDto)) => MapDerivedTypes(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithDerivedTypesOnSameMethodAndDuplicatedSourceTypeShouldIncludeAll()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            [MapDerivedType<A, D>]
            [MapDerivedType<C, B>]
            [MapDerivedType<C, D>]
            public partial object Map(object source, Type targetType);
            """,
            "class Base {}",
            "class BaseDto {}",
            "class A : Base {}",
            "class B : BaseDto {}",
            "class C : Base {}",
            "class D : BaseDto {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB(x),
                    global::A x when targetType.IsAssignableFrom(typeof(global::D)) => MapToD(x),
                    global::C x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB1(x),
                    global::C x when targetType.IsAssignableFrom(typeof(global::D)) => MapToD1(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithUserImplementedMethodsShouldBeIncluded()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            public partial object Map(object source, Type targetType);

            private partial D MapToD(B source) => new D();
            """,
            "class Base {}",
            "class BaseDto {}",
            "class A : Base {}",
            "class B : BaseDto {}",
            "class C : Base {}",
            "class D : BaseDto {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB(x),
                    global::B x when targetType.IsAssignableFrom(typeof(global::D)) => MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericUserImplementedMethodShouldBeIgnored()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            public partial object Map(object source, Type targetType);

            private partial T MapTo<T>(B source) where T : new() => new T();
            """,
            "class Base {}",
            "class BaseDto {}",
            "class A : Base {}",
            "class B : BaseDto {}",
            "class C : Base {}",
            "class D : BaseDto {}"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void InvalidSignatureAdditionalParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial object Map(A a, Type targetType, string format);",
            "class A { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void InvalidSignatureWithReferenceHandlerAdditionalParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial object Map(A a, Type targetType, [ReferenceHandler] IReferenceHandler refHanlder, string format);",
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public string StringValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowAllDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.UnsupportedMappingMethodSignature)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public Task WithReferenceHandlingEnabled()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial object Map(object source, Type targetType);

            private partial B MapToB(A source);
            private partial D MapToD(C source);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A {}",
            "class B {}",
            "class C {}",
            "class D {}"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithReferenceHandlerParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial object Map(object source, Type targetType, [ReferenceHandler] IReferenceHandler refHandler);

            private partial B MapToB(A source, [ReferenceHandler] IReferenceHandler refHandler);
            private partial D MapToD(C source, [ReferenceHandler] IReferenceHandler refHandler);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A {}",
            "class B {}",
            "class C {}",
            "class D {}"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
