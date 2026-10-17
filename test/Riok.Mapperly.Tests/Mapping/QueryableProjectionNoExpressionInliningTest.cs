namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionNoExpressionInliningTest
{
    [Fact]
    public void MapperAttributeNoExpressionInliningShouldInvoke()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);
            partial B MapSingle(A source);
            private string MapValue(string value) => value.ToUpper();
            """,
            TestSourceBuilderOptions.Default with
            {
                NoExpressionInlining = true,
            },
            "class A { public string Value { get; set; } }",
            "class B { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Value = MapValue(x.Value),
                            }
                        );
                #nullable enable
                """
            )
            .HaveMethodBody(
                "MapSingle",
                """
                var target = new global::B();
                target.Value = MapValue(source.Value);
                return target;
                """
            );
    }

    [Fact]
    public void MapperNoExpressionInliningOnBaseClassMethodShouldInvoke()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            public abstract class MapperBase
            {
                [MapperNoExpressionInlining]
                protected string MapValue(string value) => value.ToUpper();
            }

            [Mapper]
            public partial class Mapper : MapperBase
            {
                private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);
            }

            class A { public string Value { get; set; } }
            class B { public string Value { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Value = MapValue(x.Value),
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public void MapperNoExpressionInliningOnInterfaceMethodShouldInvoke()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            public interface IMapperBase
            {
                [MapperNoExpressionInlining]
                string MapValue(string value) => value.ToUpper();
            }

            [Mapper]
            public partial class Mapper : IMapperBase
            {
                private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);
            }

            class A { public string Value { get; set; } }
            class B { public string Value { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Value = ((global::IMapperBase)this).MapValue(x.Value),
                            }
                        );
                #nullable enable
                """
            );
    }

    [Fact]
    public void MapperDefaultsNoExpressionInliningShouldInvoke()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaults(NoExpressionInlining = true)]

            [Mapper]
            public partial class Mapper
            {
                private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> q);
                private string MapValue(string value) => value.ToUpper();
            }

            class A { public string Value { get; set; } }
            class B { public string Value { get; set; } }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveMapMethodBody(
                """
                #nullable disable
                        return global::System.Linq.Queryable.Select(
                            q,
                            x => new global::B()
                            {
                                Value = MapValue(x.Value),
                            }
                        );
                #nullable enable
                """
            );
    }
}
