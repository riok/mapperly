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
    public Task ClassToClassWithParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int additionalParameter);",
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; }; public int AdditionalParameter { get; set; }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassWithParametersAndUserMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int additionalParameter);
            private B MapToB(A source, int additionalParameter) => new B { StringValue = source.StringValue, AdditionalParameter = additionalParameter * 2 };
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; }; public int AdditionalParameter { get; set; }"
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

    [Fact]
    public Task QueryableProjectionWithParameterizedPropertyMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int multiplier);

            [MapProperty(nameof(A.Value), nameof(B.ProcessedValue), Use = nameof(ProcessValue))]
            private partial B MapToB(A source, int multiplier);

            private static int ProcessValue(int value, int multiplier) => value * multiplier;
            """,
            new TestSourceBuilderOptions { AutoUserMappings = false },
            "class A { public int Value { get; set; } }",
            "class B { public int ProcessedValue { get; set; } public int Value { get; set; } public int Multiplier { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task QueryableProjectionWithMultipleParameterizedPropertyMappings()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int multiplier, string prefix);

            [MapProperty(nameof(A.Value1), nameof(B.ProcessedValue1), Use = nameof(ProcessValue))]
            [MapProperty(nameof(A.Value2), nameof(B.ProcessedValue2), Use = nameof(ProcessValueWithPrefix))]
            private partial B MapToB(A source, int multiplier, string prefix);

            private static string ProcessValue(int value, int multiplier)
                => (value * multiplier).ToString();

            private static string ProcessValueWithPrefix(int value, int multiplier, string prefix)
                => prefix + (value * multiplier).ToString();
            """,
            "class A { public int Value1 { get; set; } public int Value2 { get; set; } }",
            "class B { public string ProcessedValue1 { get; set; } public string ProcessedValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task QueryableProjectionWithParameterizedAndNonParameterizedMethods()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int multiplier);

            [MapProperty(nameof(A.Value1), nameof(B.ProcessedValue1), Use = nameof(ProcessValue1))]
            [MapProperty(nameof(A.Value2), nameof(B.ProcessedValue2), Use = nameof(ProcessValue2))]
            private partial B MapToB(A source, int multiplier);

            private static int ProcessValue1(int value, int multiplier) => value * multiplier;

            private static int ProcessValue2(int value) => value * 2;
            """,
            "class A { public int Value1 { get; set; } public int Value2 { get; set; } }",
            "class B { public int ProcessedValue1 { get; set; } public int ProcessedValue2 { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task QueryableProjectionWithNestedParameterizedPropertyMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int multiplier);

            [MapProperty("Nested.Value", nameof(B.ProcessedValue), Use = nameof(ProcessValue))]
            private partial B MapToB(A source, int multiplier);

            private static int ProcessValue(int value, int multiplier) => value * multiplier;
            """,
            "class A { public C Nested { get; set; } }",
            "class B { public int ProcessedValue { get; set; } }",
            "class C { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void QueryableProjectionParameterTypeMismatchShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int multiplier);

            [MapProperty(nameof(A.Value), nameof(B.ProcessedValue), Use = nameof(ProcessValue))]
            private partial B MapToB(A source, int multiplier);

            private static string ProcessValue(int value, string wrongType)
                => value + wrongType;
            """,
            "class A { public int Value { get; set; } }",
            "class B { public string ProcessedValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void QueryableProjectionWithTooManyParametersShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source, int multiplier);

            [MapProperty(nameof(A.Value), nameof(B.ProcessedValue), Use = nameof(ProcessValue))]
            private partial B MapToB(A source, int multiplier);

            private static string ProcessValue(int value, int multiplier, string extraParam)
                => value + multiplier + extraParam;
            """,
            "class A { public int Value { get; set; } }",
            "class B { public string ProcessedValue { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveAssertedAllDiagnostics();
    }
}
