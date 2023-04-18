namespace Riok.Mapperly.Tests.Mapping;

[UsesVerify]
public class MapperTest
{
    [Fact]
    public Task SameMapperNameInMultipleNamespacesShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            namespace Test.A
            {
                [Mapper]
                internal partial class FooBarMapper
                {
                    internal partial string FooToBar(string value);
                }
            }

            namespace Test.B
            {
                [Mapper]
                internal partial class FooBarMapper
                {
                    internal partial string FooToBar(string value);
                }
            }
            """);

        return TestHelper.VerifyGenerator(source);
    }

    [Fact]
    public Task MapperInNestedClassShouldWork()
    {
        var source = TestSourceBuilder.CSharp(
            """
            using Riok.Mapperly.Abstractions;

            public static partial class CarFeature
            {
                public static partial class Mappers
                {
                    [Mapper]
                    public partial class CarMapper
                    {
                        public partial int ToInt(double value);
                    }
                }
            }
            """);

        return TestHelper.VerifyGenerator(source);
    }
}
