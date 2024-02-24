using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class GenericDerivedTypeTest
{
    [Fact]
    public void GenericWithDerivedTypes()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial TTarget Map<TTarget>(object? source);

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
                    global::Base x when typeof(TTarget).IsAssignableFrom(typeof(global::BaseDto)) => (TTarget)(object)MapDerivedTypes(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void GenericWithDerivedTypesOnSameMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            [MapDerivedType<C, D>]
            public partial TTarget Map<TTarget>(object source);
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
                    global::A x when typeof(TTarget).IsAssignableFrom(typeof(global::B)) => (TTarget)(object)MapToB(x),
                    global::C x when typeof(TTarget).IsAssignableFrom(typeof(global::D)) => (TTarget)(object)MapToD(x),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void GenericWithDerivedTypesInvalidConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            [MapDerivedType<C, D>]
            public partial TTarget Map<TTarget>(object source)
                where TTarget : B;
            """,
            "class A {}",
            "class B {}",
            "class C {}",
            "class D {}"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.DerivedTargetTypeIsNotAssignableToReturnType,
                "Derived target type D is not assignable to return type TTarget"
            )
            .HaveAssertedAllDiagnostics()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x when typeof(TTarget).IsAssignableFrom(typeof(global::B)) => (TTarget)(object)MapToB(x),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }
}
