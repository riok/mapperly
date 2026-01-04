using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class MapAdditionalSourceAttributeTest
{
    [Fact]
    public void MapAdditionalSourceWithSimpleClasses()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);",
            "class SourceA { public string A { get; set; } public string B { get; set; } }",
            "class SourceB { public string C { get; set; } public string D { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } public string C { get; set; } public string D { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = sourceA.A;
                target.B = sourceA.B;
                target.C = sourceB.C;
                target.D = sourceB.D;
                return target;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceWithExtraNonMappedParameter()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB, string d);",
            "class SourceA { public string A { get; set; } public string B { get; set; } }",
            "class SourceB { public string C { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } public string C { get; set; } public string D { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = sourceA.A;
                target.B = sourceA.B;
                target.C = sourceB.C;
                target.D = d;
                return target;
                """
            );
    }

    [Fact]
    public void PreferParameterOverAllSources()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB, string a);",
            "class SourceA { public string A { get; set; } }",
            "class SourceB { public string A { get; set; } }",
            "class Target { public string A { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.AdditionalParameterNotMapped)
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = a;
                return target;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceWithDifferentParameterNames()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target ToDestination(SourceB bigSourceB, [MapAdditionalSource] sourceB smallSource);",
            "class SourceB { public string A { get; set; } public string B { get; set; } }",
            "class sourceB { public string C { get; set; } public string D { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } public string C { get; set; } public string D { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = bigSourceB.A;
                target.B = bigSourceB.B;
                target.C = smallSource.C;
                target.D = smallSource.D;
                return target;
                """
            );
    }

    [Fact]
    public Task MapAdditionalSourceWithRecords()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class ToDestination
            {
                static partial C MapToC(A value, [MapAdditionalSource] H sourceH);
            }

            public record A(B Value1);
            public record B(int Value3);

            public record H(F Value2);
            public record F(int Value4);

            public record C(D Value1, T Value2);
            public record D(int Value3);
            public record T(int Value4);
            """
        );

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public void MultipleMapAdditionalSources()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB, [MapAdditionalSource] SourceC sourceC);",
            "class SourceA { public string A { get; set; } }",
            "class SourceB { public string B { get; set; } }",
            "class SourceC { public string C { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } public string C { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = sourceA.A;
                target.B = sourceB.B;
                target.C = sourceC.C;
                return target;
                """
            );
    }

    [Fact]
    public void AdditionalSourcePriority()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);",
            "class SourceA { public string Value { get; set; } }",
            "class SourceB { public string Value { get; set; } }",
            "class Target { public string Value { get; set; } }"
        );

        // Should prefer parameter to source member
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.AdditionalParameterNotMapped,
                "The additional mapping method parameter Value of the method ToDestination is not mapped"
            )
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.Value = sourceA.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceWithNestedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapNestedProperties(nameof(SourceB.Inner))]
            partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);
            """,
            "class SourceA { public string ValueA { get; set; } }",
            "class SourceB { public InnerB Inner { get; set; } }",
            "class InnerB { public string ValueB { get; set; } }",
            "class Target { public string ValueA { get; set; } public string ValueB { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.ValueA = sourceA.ValueA;
                target.ValueB = sourceB.Inner.ValueB;
                return target;
                """
            );
    }

    [Fact]
    public void DeeplyNestedPropertyAsArrayWithMemberNameOfSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapNestedProperties(new [] {nameof(J.Value), nameof(C.NestedValue)})]
            partial B Map(A source, [MapAdditionalSource] J sourceJ);
            """,
            "class A { public string Simple { get; set; } }",
            "class J { public C Value { get; set; } }",
            "class B { public string Simple { get; set; }, public string Id { get; set; } }",
            "class C { public D NestedValue { get; set; } }",
            "class D { public string Id { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Simple = source.Simple;
                target.Id = sourceJ.Value.NestedValue.Id;
                return target;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceWithCustomMapping()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(SourceB.CustomName), nameof(Destination.MappedName))]
            partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);
            """,
            "class SourceA { public string A { get; set; } }",
            "class SourceB { public string CustomName { get; set; } }",
            "class Target { public string A { get; set; } public string MappedName { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = sourceA.A;
                target.MappedName = sourceB.CustomName;
                return target;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceWithNullability()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target? ToDestination(SourceA? sourceA, [MapAdditionalSource] SourceB? sourceB);",
            "class SourceA { public string? A { get; set; } }",
            "class SourceB { public string? B { get; set; } }",
            "class Target { public string? A { get; set; } public string? B { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                if (sourceA == null || sourceB == null)
                    return default;
                var target = new global::Target();
                target.A = sourceA.A;
                target.B = sourceB.B;
                return target;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceWithStaticMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "static partial Target ToDestination(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);",
            "class SourceA { public string A { get; set; } }",
            "class SourceB { public string B { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.A = sourceA.A;
                target.B = sourceB.B;
                return target;
                """
            );
    }
}
