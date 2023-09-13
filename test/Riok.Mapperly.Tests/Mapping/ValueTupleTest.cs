using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class ValueTupleTest
{
    [Fact]
    public void TupleToTuple()
    {
        var source = TestSourceBuilder.Mapping("(int, string)", "(int, string)");

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void TupleToTupleWithDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("(int, string)", "(int, string)", TestSourceBuilderOptions.WithDeepCloning);

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (source.Item1, source.Item2);
            return target;
            """
            );
    }

    [Fact]
    public void TupleToDifferentTypeTuple()
    {
        var source = TestSourceBuilder.Mapping("(int, string)", "(long, int)");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = ((long)source.Item1, int.Parse(source.Item2));
            return target;
            """
            );
    }

    [Fact]
    public void NamedTupleToTuple()
    {
        var source = TestSourceBuilder.Mapping("(int A, string B)", "(int, string)");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (source.A, source.B);
            return target;
            """
            );
    }

    [Fact]
    public void TupleToNamedTuple()
    {
        var source = TestSourceBuilder.Mapping("(int, string)", "(int A, string B)");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (A: source.Item1, B: source.Item2);
            return target;
            """
            );
    }

    [Fact]
    public void NamedTupleToNamedTuple()
    {
        var source = TestSourceBuilder.Mapping("(int A, string B)", "(int A, string B)");

        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return source;");
    }

    [Fact]
    public void NamedTupleToNamedTupleWithDeepCloning()
    {
        var source = TestSourceBuilder.Mapping("(int A, string B)", "(int A, string B)", TestSourceBuilderOptions.WithDeepCloning);

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (A: source.A, B: source.B);
            return target;
            """
            );
    }

    [Fact]
    public void NamedTupleToNamedTupleWithPositionalResolve()
    {
        var source = TestSourceBuilder.Mapping("(int A, string B)", "(int C, string D)", TestSourceBuilderOptions.WithDeepCloning);

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (C: source.A, D: source.B);
            return target;
            """
            );
    }

    [Fact]
    public void PartiallyNamedTupleToPartiallyNamedTuple()
    {
        var source = TestSourceBuilder.Mapping("(int, string A)", "(int A, string)");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveSingleMethodBody(
                """
                var target = (A: int.Parse(source.A), source.A);
                return target;
                """
            );
    }

    [Fact]
    public void TupleToTupleNamedItems()
    {
        var source = TestSourceBuilder.Mapping("(int, string)", "(int Item2, string Item1)");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = (Item2: int.Parse(source.Item2), Item1: source.Item1.ToString());
                return target;
                """
            );
    }

    [Fact]
    public void TupleNamedItemsToTuple()
    {
        var source = TestSourceBuilder.Mapping("(int Item2, string Item1)", "(int, string)");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (int.Parse(source.Item1), source.Item2.ToString());
            return target;
            """
            );
    }

    [Fact]
    public void TupleToClassShouldNotDiagnosticUnmapped()
    {
        var source = TestSourceBuilder.Mapping(
            "(string, int)",
            "A",
            "class A { public string Item1 { get; set; } public int Item2 { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Item1 = source.Item1;
                target.Item2 = source.Item2;
                return target;
                """
            );
    }

    [Fact]
    public void NamedTupleToClassShouldNotDiagnosticUnmapped()
    {
        var source = TestSourceBuilder.Mapping(
            "(string A, int B)",
            "C",
            "class C { public string A { get; set; } public int B { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::C();
                target.A = source.A;
                target.B = source.B;
                return target;
                """
            );
    }

    [Fact]
    public void ClassToTuple()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "(int B, string C)",
            "public class A { public int B { get;set;} public int C {get;set;} }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (B: source.B, C: source.C.ToString());
            return target;
            """
            );
    }

    [Fact]
    public void TupleToTupleWithIgnoredSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreSource("C")]
                partial (int, string) Map((int A, string B, int C) source);
                """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (source.A, source.B);
            return target;
            """
            );
    }

    [Fact]
    public void TupleToTupleWithIgnoredSourceByPosition()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreSource("Item3")]
                partial (int, string) Map((int A, string B, int) source);
                """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (source.A, source.B);
            return target;
            """
            );
    }

    [Fact]
    public void ClassToTupleWithIgnoredSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreSource("A")]
                partial (int, int) Map(B source);
                """,
            "public class B { public int Item1 { get;set;} public int A {get;set;} public int Item2 {get;set;} }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (source.Item1, source.Item2);
            return target;
            """
            );
    }

    [Fact]
    public void IgnoredNamedSourceWithPositionalShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreSource("Item3")]
                partial (int, string) Map((int A, string B, int C) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.IgnoredSourceMemberNotFound)
            .HaveSingleMethodBody(
                """
                var target = (source.A, source.B);
                return target;
                """
            );
    }

    [Fact]
    public void TupleWithNonExistentIgnoreSourceShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreSource("D")]
                partial (int, string) Map((int A, string B) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.IgnoredSourceMemberNotFound)
            .HaveSingleMethodBody(
                """
                var target = (source.A, source.B);
                return target;
                """
            );
    }

    [Fact]
    public void InvalidTupleShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreSource("A")]
                partial (int, int) Map((int, int A) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveDiagnostic(DiagnosticDescriptors.NoConstructorFound)
            .HaveSingleMethodBody(
                """
                // Could not generate mapping
                throw new System.NotImplementedException();
                """
            );
    }

    [Fact]
    public void IgnoreTargetTuple()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreTarget("A")]
                partial (int, int A) Map((string, int A) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoConstructorFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                // Could not generate mapping
                throw new System.NotImplementedException();
                """
            );
    }

    [Fact]
    public void IgnoreTargetByPosition()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreTarget("Item1")]
                partial (int, int A) Map((string, int A) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.NoConstructorFound)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                // Could not generate mapping
                throw new System.NotImplementedException();
                """
            );
    }

    [Fact]
    public void IgnoreTargetWithNonExistentTargetShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreTarget("B")]
                partial (int, int A) Map((string, int) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.IgnoredTargetMemberNotFound)
            .HaveSingleMethodBody(
                """
                var target = (int.Parse(source.Item1), A: source.Item2);
                return target;
                """
            );
    }

    [Fact]
    public void IgnoreTargetWithNonExistentPositionalShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapperIgnoreTarget("Item3")]
                partial (int, int A) Map((string, int) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.IgnoredTargetMemberNotFound)
            .HaveSingleMethodBody(
                """
                var target = (int.Parse(source.Item1), A: source.Item2);
                return target;
                """
            );
    }

    [Fact]
    public void TupleToTupleWithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("C", "A")]
                [MapProperty("Item3", "Item2")]
                partial (int A, int) Map((int B, int C, int) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveSingleMethodBody(
                """
                var target = (A: source.C, source.Item3);
                return target;
                """
            );
    }

    [Fact]
    public Task TuplePropertyToTupleProperty()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "B",
            "class A { public (int A, int) Value { get; set; } }",
            "class B { public (string A, int) Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyShouldMapToTupleField()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("Item2", "Item1.Value")]
                private partial (A, int) Map((B, string) source);
                """,
            "class A { public int Value { get; set; } }",
            "class B { public int Value { get; set; } }"
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyShouldMapNestedTuple()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("Item2", "Item1.Item1")]
                private partial ((int, int), int) Map(((int, int), string) source);
                """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyShouldMapNamedNestedTuple()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("Item1", "C")]
                [MapProperty("B", "C.D")]
                private partial ((int, int D) C, int) Map(((int F, int G), string B) source);
                """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapPropertyShouldMapFieldAndNestedTuple()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("A", "F")]
                [MapProperty("D", "E")]
                [MapProperty("D", "F.H")]
                private partial (string E, (long G, int H) F) Map(((int B, int C) A, string D) source);
                """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void TupleToValueTuple()
    {
        var source = TestSourceBuilder.Mapping("(int A, string B)", "ValueTuple<int, string>");

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
            var target = (source.A, source.B);
            return target;
            """
            );
    }

    [Fact]
    public void QueryableTupleToQueryableTuple()
    {
        var source = TestSourceBuilder.Mapping("System.Linq.IQueryable<(int A, string B)>", "System.Linq.IQueryable<(int, string)>");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                #nullable disable
                        return System.Linq.Queryable.Select(source, x => new global::System.ValueTuple<int, string>(x.A, x.B));
                #nullable enable
                """
            );
    }

    [Fact]
    public void QueryableTupleToIQueryableValueTuple()
    {
        var source = TestSourceBuilder.Mapping(
            "System.Linq.IQueryable<(int A, string B)>",
            "System.Linq.IQueryable<ValueTuple<int, string>>"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                #nullable disable
                        return System.Linq.Queryable.Select(source, x => new global::System.ValueTuple<int, string>(x.A, x.B));
                #nullable enable
                """
            );
    }

    [Fact]
    public void TupleToTupleWithManyMapPropertyShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("B", "A")]
            [MapProperty("B", "A")]
            partial (int A, int) Map((int, string B) source);
            """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.MultipleConfigurationsForConstructorParameter);
    }

    [Fact]
    public void TupleToTupleWithMapPropertyWithImplicitNameShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
                [MapProperty("Item2", "Item1")]
                partial (int A, int B) Map((int C, int D) source);
                """
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.ConfiguredMappingTargetMemberNotFound);
    }

    [Fact]
    public Task SimilarTupleMappingShouldCreateSeparateMethods()
    {
        var source = TestSourceBuilder.MapperWithBody(
            """
            private partial (string A, string B) Map((int, int) src);
            private partial (string A, string B) Map((int B, int A) src);
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void TupleMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "(int, string)",
            "(string, int)",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.Tuple)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveSingleMethodBody(
                """
                // Could not generate mapping
                throw new System.NotImplementedException();
                """
            );
    }

    [Fact]
    public void ClassToTupleWithNoMappingsShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping("A", "(int, int)", "public class A { }");

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotFound)
            .HaveSingleMethodBody(
                """
                // Could not generate mapping
                throw new System.NotImplementedException();
                """
            );
    }
}
