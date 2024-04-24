using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class QueryableProjectionLoopTest
{
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
    public Task SetRecursionDepthToZero()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                MaxRecursionDepth = 0
            },
            "class A { public A? Parent { get; set; } }",
            "class B { public B? Parent { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task SetRecursionDepthToOne()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                MaxRecursionDepth = 1
            },
            "class A { public A? Parent { get; set; } }",
            "class B { public B? Parent { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task SetRecursionDepthToTwo()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            TestSourceBuilderOptions.Default with
            {
                MaxRecursionDepth = 2
            },
            "class A { public A? Parent { get; set; } }",
            "class B { public B? Parent { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MethodAttributeSetRecursionDepthForLoop()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> src);

            [MapperMaxRecursionDepth(2)]
            partial B Map(A src);
            """,
            "class A { public A? Parent { get; set; } }",
            "class B { public B? Parent { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MethodAttributeOverridesClassSetRecursionDepthForLoop()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> src);
            [MapperMaxRecursionDepth(2)] partial B Map(A src);
            """,
            TestSourceBuilderOptions.Default with
            {
                MaxRecursionDepth = 4
            },
            "class A { public A? Parent { get; set; } }",
            "class B { public B? Parent { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AssemblyDefaultShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaultsAttribute(MaxRecursionDepth = 2)]
            [Mapper()]
            public partial class MyMapper
            {
                partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> src);
            }

            class A { public A? Parent { get; set; } }

            class B { public B? Parent { get; set; } }
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task AttributeShouldOverrideAssemblyDefault()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [assembly: MapperDefaultsAttribute(MaxRecursionDepth = 2)]
            [Mapper(MaxRecursionDepth = 4)]
            public partial class MyMapper
            {
                partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> src);
            }

            class A { public A? Parent { get; set; } }

            class B { public B? Parent { get; set; } }
            """
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
    public Task IndirectReferenceLoop()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<A>",
            "System.Linq.IQueryable<B>",
            "class A { public string StringValue { get; set; } public C Parent { get; set; } }",
            "class B { public string StringValue { get; set; } public D Parent { get; set; } }",
            "class C { public A Parent { get; set; } }",
            "class D {  public B Parent { get; set; } }"
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
