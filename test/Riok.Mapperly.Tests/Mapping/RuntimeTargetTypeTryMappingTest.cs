namespace Riok.Mapperly.Tests.Mapping;

public class RuntimeTargetTypeTryMappingTest
{
    [Fact]
    public Task WithNullableObjectSourceAndTargetTypeShouldIncludeNullables()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial bool TryMap(object? source, Type targetType, out object? result);

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
    public Task WithGenericSourceAndTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial bool TryMap<TSource, TTarget>(IQueryable<TSource> source, out IQueryable<TTarget> result);

            private partial IQueryable<B> ProjectToB(IQueryable<A> q);
            private partial IQueryable<D> ProjectToD(IQueryable<C> q);
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
            public partial bool TryMap(object source, Type targetType, out object result);

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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::A x when targetType.IsAssignableFrom(typeof(global::B)):
                        result = MapToB(x);
                        return true;
                    default:
                        return false;
                }
                """
            );
    }

    [Fact]
    public void WithSubsetSourceTypeAndObjectTargetTypeShouldWork()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial bool TryMap(Base1Source source, Type targetType, out Base1Target result);

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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::A x when targetType.IsAssignableFrom(typeof(global::B)):
                        result = MapToB(x);
                        return true;
                    default:
                        return false;
                }
                """
            );
    }

    [Fact]
    public void WithTypeHierarchyShouldPreferMostSpecificMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial bool TryMap(object source, Type targetType, out object result);

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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::A x when targetType.IsAssignableFrom(typeof(global::C)):
                        result = MapAToC(x);
                        return true;
                    case global::B x when targetType.IsAssignableFrom(typeof(global::C)):
                        result = MapBToC(x);
                        return true;
                    case global::Base2 x when targetType.IsAssignableFrom(typeof(global::C)):
                        result = MapB2ToC(x);
                        return true;
                    case global::Base1 x when targetType.IsAssignableFrom(typeof(global::C)):
                        result = MapB1ToC(x);
                        return true;
                    default:
                        return false;
                }
                """
            );
    }

    [Fact]
    public void WithDerivedTypesShouldUseBaseType()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial bool TryMap(object source, Type targetType, out object result);

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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::Base x when targetType.IsAssignableFrom(typeof(global::BaseDto)):
                        result = MapDerivedTypes(x);
                        return true;
                    default:
                        return false;
                }
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
            public partial bool TryMap(object source, Type targetType, out object result);
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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::A x when targetType.IsAssignableFrom(typeof(global::B)):
                        result = MapToB(x);
                        return true;
                    case global::A x when targetType.IsAssignableFrom(typeof(global::D)):
                        result = MapToD(x);
                        return true;
                    case global::C x when targetType.IsAssignableFrom(typeof(global::B)):
                        result = MapToB1(x);
                        return true;
                    case global::C x when targetType.IsAssignableFrom(typeof(global::D)):
                        result = MapToD1(x);
                        return true;
                    default:
                        return false;
                }
                """
            );
    }

    [Fact]
    public void WithUserImplementedMethodsShouldBeIncluded()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            public partial bool TryMap(object source, Type targetType, out object result);

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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::A x when targetType.IsAssignableFrom(typeof(global::B)):
                        result = MapToB(x);
                        return true;
                    case global::B x when targetType.IsAssignableFrom(typeof(global::D)):
                        result = MapToD(x);
                        return true;
                    default:
                        return false;
                }
                """
            );
    }

    [Fact]
    public void WithGenericUserImplementedMethodShouldBeIgnored()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapDerivedType<A, B>]
            public partial bool TryMap(object source, Type targetType, out object result);

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
            .HaveTryMapMethodBody(
                """
                result = default;
                switch (source)
                {
                    case global::A x when targetType.IsAssignableFrom(typeof(global::B)):
                        result = MapToB(x);
                        return true;
                    default:
                        return false;
                }
                """
            );
    }

    [Fact]
    public Task WithReferenceHandlingEnabled()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial bool TryMap(object source, Type targetType, out object result);

            private partial B MapToB(A source);
            private partial D MapToD(C source);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public int IntValue { get; set; } }",
            "class B { public int IntValue { get; set; } }",
            "class C { public int IntValue { get; set; } }",
            "class D { public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task WithReferenceHandlerParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial bool TryMap(object source, Type targetType, [ReferenceHandler] IReferenceHandler refHandler, out object result);

            private partial B MapToB(A source, [ReferenceHandler] IReferenceHandler refHandler);
            private partial D MapToD(C source, [ReferenceHandler] IReferenceHandler refHandler);
            """,
            TestSourceBuilderOptions.WithReferenceHandling,
            "class A { public int IntValue { get; set; } }",
            "class B { public int IntValue { get; set; } }",
            "class C { public int IntValue { get; set; } }",
            "class D { public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
