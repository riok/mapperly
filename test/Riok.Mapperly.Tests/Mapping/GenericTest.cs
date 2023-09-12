namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class GenericTest
{
    [Fact]
    public Task WithGenericSourceAndTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                private partial TTarget Map<TSource, TTarget>(TSource source);

                private partial B MapToB(A source);
                private partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithGenericSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source);

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x => MapToB(x),
                    global::C x => MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeConstraints()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : A;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x => MapToB(x),
                    global::C x => MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeNotNullConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : notnull;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x => MapToB(x),
                    global::C x => MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeValueTypeConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : struct;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x => MapToB(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeUnmanagedConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : unmanaged;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::A x => MapToB(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeReferenceTypeConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : class;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::C x => MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeReferenceTypeConstraintInNullableDisabledContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : class;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.DisabledNullable)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::C x => MapToD(x),
                    null => default(object),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceTypeNullableReferenceTypeConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial object Map<TSource>(TSource source)
                    where TSource : class?;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::C x => MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(object)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceAndTargetTypeNullableReferenceTypeConstraint()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial TTarget Map<TSource, TTarget>(TSource source)
                    where TSource : class?
                    where TTarget : class?;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::C x when typeof(TTarget).IsAssignableFrom(typeof(global::D)) => (TTarget)(object)MapToD(x),
                    null => default(TTarget),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial TTarget Map<TTarget>(object source);

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
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
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericTargetTypeConstraints()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial TTarget Map<TTarget>(object source)
                    where TTarget : D;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::C x when typeof(TTarget).IsAssignableFrom(typeof(global::D)) => (TTarget)(object)MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public void WithGenericSourceAndTargetTypeConstraints()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial TTarget Map<TSource, TTarget>(TSource source)
                    where TSource : C
                    where TTarget : D;

                partial B MapToB(A source);
                partial D MapToD(C source);
                """,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                return source switch
                {
                    global::C x when typeof(TTarget).IsAssignableFrom(typeof(global::D)) => (TTarget)(object)MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
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
            public partial TTarget Map<TSource, TTarget>(TSource source);

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
                    global::A x when typeof(TTarget).IsAssignableFrom(typeof(global::B)) => (TTarget)(object)MapToB(x),
                    global::B x when typeof(TTarget).IsAssignableFrom(typeof(global::D)) => (TTarget)(object)MapToD(x),
                    null => throw new System.ArgumentNullException(nameof(source)),
                    _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {typeof(TTarget)} as there is no known type mapping", nameof(source)),
                };
                """
            );
    }

    [Fact]
    public Task WithGenericSourceAndTargetAndEnabledReferenceHandling()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                private partial TTarget Map<TSource, TTarget>(TSource source);

                private partial B MapToB(A source);
                private partial D MapToD(C source);
                """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithGenericSourceAndTargetAndEnabledReferenceHandlingAndParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                private partial TTarget Map<TSource, TTarget>(TSource source, [ReferenceHandler] IReferenceHandler refHandler);

                private partial B MapToB(A source, [ReferenceHandler] IReferenceHandler refHandler);
                private partial D MapToD(C source, [ReferenceHandler] IReferenceHandler refHandler);
                """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "record struct A(string Value);",
            "record struct B(string Value);",
            "record C(string Value1);",
            "record D(string Value1);"
        );
        return TestHelper.VerifyGenerator(source);
    }
}
