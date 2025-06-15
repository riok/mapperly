using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionTest
{
    [Fact]
    public Task ClassToClass()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassMultipleProperties()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } public char CharValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } public char CharValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNested()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string StringValue { get; set; } public C OtherValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D OtherValue { get; set; } }",
            "class C { public int IntValue { get; set; } }",
            "class D { public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNestedMemberAttribute()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);
            [MapNestedProperties(nameof(A.Value))] private partial B MapToB(A source);
            """,
            "class A { public C Value { get; set; } }",
            "class B { public int Id { get; set; } }",
            "class C { public int Id { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassWithConfigs()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);
            [MapProperty("StringValue", "StringValue2")] private partial B MapToB(A source);
            [MapProperty("LongValue", "IntValue")] private partial D MapToD(C source);
            """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue2 { get; set; } public D NestedValue { get; set; } }",
            "class C { public long LongValue { get; set; } public E NestedValue { get; set; } }",
            "class D { public int IntValue { get; set; } public F NestedValue { get; set; } }",
            "class E { public short ShortValue { get; set; } }",
            "class F { public short ShortValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task QueryablePropertyWithStringFormat()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            [MapProperty("Value", "Value", StringFormat = "C")]
            private partial B MapPrivate(A source);",
            """,
            "class A { public int Value { get; set; } }",
            "class B { public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task RecordToRecordManualFlatteningInsideList()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private partial D Map(C source) => source.Value;
            """,
            "record A(Guid Id, List<C> Values);",
            "record B(string Id, List<D> Values);",
            "record D(int IntValue);",
            "record C(D Value);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task RecordToRecordManualMappingWithGlobalTypeArgsInInliningMappingMethod()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial IQueryable<B> ProjectAToB(IQueryable<A> query);

            public DateTimeOffset MapDateTimeToDateTimeOffset(DateTime value) =>
                value != DateTime.MinValue ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)) : DateTimeOffset.MinValue;
            """,
            "public sealed record A(string Name, DateTime ChangedOn);",
            "public sealed record B(string Name, DateTimeOffset ChangedOn);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task RecordToRecordManualListMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private partial List<D> Map(List<C> source) => source.Select(x => x.Value).OrderBy(x => x).ToList<D>();
            """,
            "record A(Guid Id, List<C> Values);",
            "record B(string Id, List<D> Values);",
            "enum D(V1, V2, V3);",
            "record C(D Value);"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceLoopInitProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public A? Parent { get; set; } public int IntValue { get; set; } }",
            "class B { public B? Parent { get; set; } public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceLoopCtor()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public A? Parent { get; set; } public int IntValue { get; set; } }",
            "class B { public B(B? parent) {} public int IntValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CtorShouldSkipUnmatchedOptionalParameters()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string StringValue { get; } public int IntValue { get; } }",
            "class B { public B(string stringValue, int optionalArgument = 10, int intValue = 20) {} public B(string stringValue) {} public int IntValue { set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CtorWithPathMappingShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial IQueryable<B> Map(IQueryable<A> source);

            [MapProperty("Value1", "ValueC.Value3")]
            private partial B MapObj(A source);
            """,
            "class A { public int Value1 { get; set; } public C ValueC { get; set; } }",
            "class B(int value1, C valueC) { public int Value1 { get; set; } public C ValueC { get; set; } }",
            "class C { public int Value2 { get; set; } public int Value3 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithReferenceHandlingShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<long>",
            "System.Linq.IQueryable<int>",
            TestSourceBuilderOptions.WithReferenceHandling
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.QueryableProjectionMappingsDoNotSupportReferenceHandling)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public async Task TopLevelUserImplemented()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

            private B Map(A source) => new B(10) { Value2 = 11, Value3 = "foo bar" };
            """,
            "class A;",
            "class B(int value) { public int Value2 { get; set; } public string Value3 { get; set; } }"
        );

        await TestHelper.VerifyGenerator(source);
    }
}
