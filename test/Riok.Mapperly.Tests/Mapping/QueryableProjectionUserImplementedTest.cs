namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionUserImplementedTest
{
    [Fact]
    public Task ClassToClassInlinedExpression()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v) => new D { Value = v.Value + "-mapped" };
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassInlinedSingleStatement()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v)
            {
                return new D { Value = v.Value + "-mapped" };
            }
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassInlinedSingleDeclaration()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v)
            {
                var dest = new D { Value = v.Value + "-mapped" };
                return dest;
            }
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNonInlinedMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v)
            {
                var dest = new D();
                dest.Value = v.Value + "-mapped";
                return dest;
            }
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedOrdering()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private List<C> Order(List<C> v)
                => v.OrderBy(x => x.Value).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedParenthesizedLambdaOrdering()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private List<C> Order(List<C> v)
                => v.OrderBy((x) => x.Value).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedOrderingWithMappingAndParameterHiding()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private partial D MapToD(C source);

            private List<D> Order(List<C> x)
                => x.OrderBy(x => x.Value).Select(x => MapToD(x)).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<D> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedOrderingWithTwoNestedMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private partial D MapToD(C source);
            private string MapString(string s) => s + "-mod";

            private List<D> Order(List<C> v)
                => v.OrderBy(x => x.Value).Select(x => MapToD(x)).ToList();
            """,
            "class A { public string StringValue { get; set; } public List<C> NestedValues { get; set; } }",
            "class B { public string StringValue { get; set; } public List<D> NestedValues { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedWithUsingMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private static DateTimeOffset MapToDateTimeOffset(DateTime dateTime)
                => new DateTimeOffset(dateTime, TimeSpan.Zero);
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public DateTimeOffset Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedWithTargetTypeNew()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private static DateTimeOffset MapToDateTimeOffset(DateTime dateTime)
                => new(dateTime, TimeSpan.Zero);
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public DateTimeOffset Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassUserImplementedWithTargetTypeNewInitializer()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private static C MapIt(DateTime dateTime)
                => new(1) { Value2 = 10 };
            """,
            "class A { public DateTime Value { get; set; } }",
            "class B { public C Value { get; set; } }",
            "class C(int value1) { public int Value2 { set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedFlatteningWithCallToAnotherMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<D> Map(IQueryable<A> source);

            private static E Map(B source) => Map(source.Child);
            private static partial E Map(C source);
            """,
            "public record A(B Value);",
            "public record B(C Child);",
            "public record C(string Name);",
            "public record D(E Value);",
            "public record E(string Name);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedNullableValueTypeToNonNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<B> Map(this IQueryable<A> query);

            private static int MapValue(int? value) => value ?? 0;
            """,
            "public record A(int? Value);",
            "public record B(int Value);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedAttributedNullableValueTypeToNullable()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<B> Map(this IQueryable<A> query);

            [MapProperty(nameof(A.Value), nameof(B.Value), Use = nameof(MapValue))]
            private static partial B Map(A src);

            [UserMapping(Default = false)]
            private static decimal? MapValue(decimal? value) => value ?? 1;
            """,
            "public class A { public decimal? Value { get; set; } }",
            "public class B { public decimal? Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedAttributedNullableValueTypeToNullableMemberTypeMismatch()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial IQueryable<B> Map(this IQueryable<A> query);

            [MapProperty(nameof(A.Value), nameof(B.Value), Use = nameof(MapValue))]
            private static partial B Map(A src);

            [UserMapping(Default = false)]
            private static decimal MapValue(decimal? value) => value ?? 1;
            """,
            "public class A { public decimal? Value { get; set; } }",
            "public class B { public decimal? Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedTwoNestedProjectionsShouldReuseInlinedMapping()
    {
        // https://github.com/riok/mapperly/issues/1965
        var source = TestSourceBuilder.CSharp(
            """
            using System;
            using System.Collections.Generic;
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            [Mapper]
            public static partial class Mapper
            {
                public static partial IQueryable<VendorResponse> ProjectToDto(this IQueryable<Vendor> query);
                public static partial IQueryable<ScopeResponse> ProjectToDto(this IQueryable<Scope> q);

                [MapPropertyFromSource(nameof(CountryListResponse.Name), Use = nameof(GetCountryLocalizedName))]
                public static partial CountryListResponse ToCountryListDto(this Country q);

                private static IReadOnlyCollection<CountryListResponse> MapScopeCountries(ICollection<ScopeCountry> countries)
                {
                    return countries.Select(c => ToCountryListDto(c.Country)).ToList();
                }

                private static IReadOnlyCollection<CountryListResponse> MapVendorCountries(ICollection<VendorCountry> models)
                {
                    return models.Select(x => ToCountryListDto(x.Country)).ToList();
                }

                private static string GetCountryLocalizedName(Country x)
                {
                    return x.LocalizedNames.FirstOrDefault()!.Name ?? x.Name;
                }
            }

            public class Vendor
            {
                public string VendorName { get; set; } = null!;
                public ICollection<VendorCountry> Countries { get; set; } = [];
            }

            public class VendorCountry
            {
                public Guid VendorId { get; set; }
                public Vendor Vendor { get; set; } = null!;
                public Guid CountryId { get; set; }
                public Country Country { get; set; } = null!;
            }

            public class Scope
            {
                public Guid Id { get; set; }
                public ICollection<ScopeCountry> Countries { get; set; } = [];
            }

            public class ScopeCountry
            {
                public Guid ScopeId { get; set; }
                public Scope Scope { get; set; } = null!;
                public Guid CountryId { get; set; }
                public Country Country { get; set; } = null!;
            }

            public class Country
            {
                public string Code { get; set; } = null!;
                public string Name { get; set; } = null!;
                public ICollection<LocalizedName> LocalizedNames { get; set; } = [];
            }

            public class LocalizedName
            {
                public string LanguageCode { get; set; } = null!;
                public string Name { get; set; } = null!;
            }

            public class CountryListResponse
            {
                public required string Code { get; set; }
                public required string Name { get; set; }
            }

            public class CountryListResponse2
            {
                public required string Code { get; set; }
                public required string Name { get; set; }
            }

            public class VendorResponse
            {
                public required string VendorName { get; set; }
                public IReadOnlyCollection<CountryListResponse> Countries { get; set; } = [];
            }

            public class ScopeResponse
            {
                public required Guid Id { get; set; }
                public IReadOnlyCollection<CountryListResponse> Countries { get; set; } = [];
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedInExtensionBlockShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;
            using System.Linq;

            [Mapper]
            partial class Mapper
            {
                [MapProperty(nameof(MyEntity.Id), nameof(MyEntityDto.Id), Use = nameof(IntToInt))]
                internal partial MyEntityDto MapToDto(MyEntity source);

                internal partial IQueryable<MyEntityDto> ProjectToDto(IQueryable<MyEntity> source);

                private static int IntToInt(int source) => source.WithOneAdded();
            }

            class MyEntity
            {
                public int Id { get; set; }
            }

            class MyEntityDto
            {
                public int Id { get; set; }
            }

            static class Extensions
            {
                extension(int source)
                {
                    internal int WithOneAdded() => source + 1;
                }
            }
            """
        );
        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedWithParenthesizedLambdaParameterNameConflict()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private D MapToD(C v) => new D { Value = v.Items.Select((x, i) => x + i).FirstOrDefault() };
            """,
            "class A { public C Nested { get; set; } }",
            "class B { public D Nested { get; set; } }",
            "class C { public System.Collections.Generic.List<string> Items { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedWithLambdaParameterNameConflict()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial System.Linq.IQueryable<BlogDto> ProjectToDto(this System.Linq.IQueryable<Blog> q);

            [MapPropertyFromSource(nameof(BlogDto.Id), Use = nameof(MapId))]
            public static partial BlogDto BlogMap(Blog blog);

            private static int MapId(Blog blog) => blog.Posts.Count(x => x.Id == blog.Posts.Count);
            """,
            "public record Blog(System.Collections.Generic.List<Post> Posts);",
            "public record Post(int Id);",
            "public record BlogDto(int Id);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task UserImplementedWithMultipleLambdaParameterNameConflicts()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial System.Linq.IQueryable<BlogDto> ProjectToDto(this System.Linq.IQueryable<Blog> q);

            [MapPropertyFromSource(nameof(BlogDto.Id), Use = nameof(MapId))]
            public static partial BlogDto BlogMap(Blog blog);

            private static int MapId(Blog blog) => blog.Posts.Where(x => x.Id > 0).Count(x => x.Id == blog.Posts.Count);
            """,
            "public record Blog(System.Collections.Generic.List<Post> Posts);",
            "public record Post(int Id);",
            "public record BlogDto(int Id);"
        );

        return TestHelper.VerifyGenerator(source);
    }
}
