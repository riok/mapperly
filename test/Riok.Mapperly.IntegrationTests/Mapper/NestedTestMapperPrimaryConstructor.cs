#if NET8_0_OR_GREATER
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public partial class NestedTestMapperPrimaryConstructor(string test)
    {
        private readonly string _test = test;

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
#endif
