using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public partial class NestedTestMapperPrimaryConstructor(string test)
    {
        public partial class TestNesting
        {
            [Mapper]
            public static partial class NestedMapper
            {
                public static partial int ToInt(decimal value);
            }
        }
    }
}
