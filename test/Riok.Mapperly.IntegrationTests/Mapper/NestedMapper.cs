using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class NestedTestMapper
    {
        public static partial class TestNesting
        {
            [Mapper]
            public static partial class NestedMapper
            {
                public static partial decimal ToDecimal(int value);
            }
        }
    }
}
