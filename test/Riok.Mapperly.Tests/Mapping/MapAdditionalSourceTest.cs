using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class MapAdditionalSourceTest
{
    [Fact]
    public void SimpleClasses()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);",
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
    public void MultipleAdditionalSources()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB, [MapAdditionalSource] SourceC sourceC);",
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
    public void MainSourceHasPriorityOverAdditionalSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);",
            "class SourceA { public string Value { get; set; } }",
            "class SourceB { public string Value { get; set; } }",
            "class Target { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.Value = sourceA.Value;
                return target;
                """
            );
    }

    [Fact]
    public void StaticMapper()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "static partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);",
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

    [Fact]
    public void WithMapProperty()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty(nameof(SourceB.CustomName), nameof(Target.MappedName))]
            partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);
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
    public void WithMapNestedProperties()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapNestedProperties(nameof(SourceB.Inner))]
            partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB);
            """,
            "class SourceA { public string ValueA { get; set; } }",
            "class SourceB { public Inner Inner { get; set; } }",
            "class Inner { public string ValueB { get; set; } }",
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
    public void NullableSourceAndAdditionalSource()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target? Map(SourceA? sourceA, [MapAdditionalSource] SourceB? sourceB);",
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
    public void FirstAdditionalSourceHasPriorityOverSecond()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB, [MapAdditionalSource] SourceC sourceC);",
            "class SourceA { }",
            "class SourceB { public string Value { get; set; } }",
            "class SourceC { public string Value { get; set; } }",
            "class Target { public string Value { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::Target();
                target.Value = sourceB.Value;
                return target;
                """
            );
    }

    [Fact]
    public void WithExistingTarget()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial void Map(SourceA sourceA, [MapAdditionalSource] SourceB sourceB, [MappingTarget] Target target);",
            "class SourceA { public string A { get; set; } }",
            "class SourceB { public string B { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                target.A = sourceA.A;
                target.B = sourceB.B;
                """
            );
    }

    [Fact]
    public void MapAdditionalSourceOnSourceParameterShouldDiagnostic()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            "partial Target Map([MapAdditionalSource] SourceA sourceA, SourceB sourceB);",
            "class SourceA { public string A { get; set; } }",
            "class SourceB { public string B { get; set; } }",
            "class Target { public string A { get; set; } public string B { get; set; } }"
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(
                DiagnosticDescriptors.MapAdditionalSourceNotApplicable,
                "MapAdditionalSource attribute cannot be applied to the parameter sourceA of the method Map because it is the source, target, or reference handler parameter"
            );
    }

    [Fact]
    public Task WithRecords()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper]
            public partial class MyMapper
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
}
