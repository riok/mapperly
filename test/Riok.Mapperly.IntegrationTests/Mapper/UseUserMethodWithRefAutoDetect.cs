using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public static partial class UseUserMethodWithRefAutoDetect
    {
        public static partial void Map([MappingTarget] ITestGenericValue<string> target, ITestGenericValue<Optional<string>> source);

        private static void MapOptional(Optional<string> src, ref string target)
        {
            if (src.HasValue)
            {
                target = src.Value;
            }
        }
    }
}
