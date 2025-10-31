using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public partial interface INestedTestMapper
    {
        [Mapper]
        public static partial class NestedMapper
        {
            public static partial decimal ToDecimal(int value);
        }
    }
}
