using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class ExpressionMappingTest
{
    [Fact]
    public Task ClassToClass()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassMultipleProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
            "class A { public string StringValue { get; set; } public int IntValue { get; set; } public char CharValue { get; set; } }",
            "class B { public string StringValue { get; set; } public int IntValue { get; set; } public char CharValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ClassToClassNested()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
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
            partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
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
            private partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            [MapProperty("StringValue", "StringValue2")] private partial B MapToB(A source);
            [MapProperty("LongValue", "IntValue")] private partial D MapToD(C source);
            """,
            TestSourceBuilderOptions.AllConversions,
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
    public Task RecordToRecordManualFlatteningInsideList()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            private partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();

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
    public Task ReferenceLoopInitProperty()
    {
        // Use non-init property to avoid reference loop errors
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
            "class A { public C? Nested { get; set; } public int IntValue { get; set; } }",
            "class B { public D? Nested { get; set; } public int IntValue { get; set; } }",
            "class C { public int Value { get; set; } }",
            "class D { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task NestedWithCtor()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
            "class A { public C? Nested { get; set; } public int IntValue { get; set; } }",
            "class B { public B(D? nested) {} public int IntValue { get; set; } }",
            "class C { public int Value { get; set; } }",
            "class D { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task CtorShouldSkipUnmatchedOptionalParameters()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
            "class A { public string StringValue { get; } public int IntValue { get; } }",
            "class B { public B(string stringValue, int optionalArgument = 10, int intValue = 20) {} public B(string stringValue) {} public int IntValue { set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void WithReferenceHandlingShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<long, int>> Map();
            """,
            TestSourceBuilderOptions.AllConversionsWithReferenceHandling
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
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();

            private B MapA(A source) => new B(10) { Value2 = 11, Value3 = "foo bar" };
            """,
            "class A;",
            "class B(int value) { public int Value2 { get; set; } public string Value3 { get; set; } }"
        );

        await TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ExpressionWithStringFormat()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();

            [MapProperty("Value", "Value", StringFormat = "C")]
            private partial B MapPrivate(A source);
            """,
            "class A { public int Value { get; set; } }",
            "class B { public string Value { get; init; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task StaticMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public static partial System.Linq.Expressions.Expression<System.Func<A, B>> CreateProjection();
            """,
            TestSourceBuilderOptions.Default with
            {
                Static = true,
            },
            "class A { public string StringValue { get; set; } }",
            "class B { public string StringValue { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task RecordToRecordDisabledNullableContext()
    {
        // see https://github.com/riok/mapperly/issues/1196
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();
            """,
            "public record A(string Value);",
            "public record B(string Value);"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task RecordToRecordMemberMappingDisabledNullableContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();

            [MapProperty(nameof(A.Value), nameof(B.OtherValue)]
            private partial B MapPrivate(A source);
            """,
            "public record A(string Value);",
            "public record B(string OtherValue);"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }

    [Fact]
    public Task ClassToClassMemberMappingDisabledNullableContext()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            public partial System.Linq.Expressions.Expression<System.Func<A, B>> Map();

            [MapProperty(nameof(A.Value), nameof(B.OtherValue)]
            private partial B MapPrivate(A source);
            """,
            "public class A { public string Value {get; set;} }",
            "public class B { public string OtherValue {get; set;} }"
        );

        return TestHelper.VerifyGenerator(source, TestHelperOptions.DisabledNullable);
    }
}
