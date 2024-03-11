using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class IgnoreAttributeTest
{
    private readonly string _classA = TestSourceBuilder.CSharp(
        """
        class A
        {
            public int Value { get; set; }

            [MapperIgnore]
            public int Ignored { get; set; }
        }
        """
    );

    private readonly string _classB = TestSourceBuilder.CSharp(
        """
        class B
        {
            public int Value { get; set; }

            [MapperIgnore]
            public int Ignored { get; set; }
        }
        """
    );

    [Fact]
    public void ClassAttributeIgnoreMember()
    {
        var source = TestSourceBuilder.Mapping("A", "B", TestSourceBuilderOptions.Default, _classA, _classB);

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                return target;
                """
            );
    }

    [Fact]
    public void MapPropertyOverridesIgnoreMember()
    {
        var source = TestSourceBuilder.MapperWithBodyAndTypes(
            """
            [MapProperty("Ignored", "Ignored")]
            partial B Map(A source);
            """,
            _classA,
            _classB
        );

        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::B();
                target.Value = source.Value;
                target.Ignored = source.Ignored;
                return target;
                """
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.IgnoredSourceMemberExplicitlyMapped,
                "The source member Ignored on A is ignored, but is also mapped by the MapPropertyAttribute"
            )
            .HaveDiagnostic(
                DiagnosticDescriptors.IgnoredTargetMemberExplicitlyMapped,
                "The target member Ignored on B is ignored, but is also mapped by the MapPropertyAttribute"
            )
            .HaveAssertedAllDiagnostics();
    }
}
