using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
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
    public Task ClassToClassWithConfigs()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);
                [MapProperty("StringValue", "StringValue2")] partial B MapToB(A source);
                [MapProperty("LongValue", "IntValue")] partial D MapToD(C source);
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
    public Task ClassToClassWithUserImplemented()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source);

                D MapToD(C v) => new D { Value = v.Value + "-mapped" };
                """,
            "class A { public string StringValue { get; set; } public C NestedValue { get; set; } }",
            "class B { public string StringValue { get; set; } public D NestedValue { get; set; } }",
            "class C { public string Value { get; set; } }",
            "class D { public string Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceLoopInitProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public A? Parent { get; set; } }",
            "class B { public B? Parent { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task ReferenceLoopCtor()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public A? Parent { get; set; } }",
            "class B { public B(B? parent) {} }"
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
}
