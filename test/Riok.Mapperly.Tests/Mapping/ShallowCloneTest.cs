using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Tests.Mapping;

public class ShallowCloneTest
{
    [Fact]
    public void ShallowCloneShouldNotReturnOriginalInstance()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "A",
            TestSourceBuilderOptions.WithShallowCloning,
            """
            class A
            {
                public int Value { get; set; }
                public List<string> List { get; set; }
                public A NestedReference { get; set; }
            }"
            """
        );
        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                target.List = source.List;
                target.NestedReference = Map(source.NestedReference);
                return target;
                """
            );
    }

    [Fact]
    public void ShallowCloneWithNoUserMappingsShouldDirectAssignAllProperties()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            [Mapper(CloningStrategy = CloningStrategy.ShallowCloning)]
            public partial class Mapper
            {
                [UserMapping(Default = false)]
                public partial A Map(A source);
            }

            class A
            {
                public int Value { get; set; }
                public List<string> List { get; set; }
                public A NestedReference { get; set; }
            }
            """
        );

        TestHelper
            .GenerateMapper(source)
            .Should()
            .HaveSingleMethodBody(
                """
                var target = new global::A();
                target.Value = source.Value;
                target.List = source.List;
                target.NestedReference = source.NestedReference;
                return target;
                """
            );
    }
}
