using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class NestedTestMapper
    {
        public static partial class TestNesting
        {
            [Mapper(EnabledConversions = MappingConversionType.ExplicitCast)]
            public static partial class NestedMapper
            {
                public static partial int ToInt(decimal value);
            }
        }
    }
}
