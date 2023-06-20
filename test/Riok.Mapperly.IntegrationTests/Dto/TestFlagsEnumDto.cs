using System;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    [Flags]
    public enum TestFlagsEnumDto
    {
        V1 = 1 << 0,
        V2 = 1 << 1,

        // use another name to test mapping by value
        V3 = 1 << 2,
    }
}
