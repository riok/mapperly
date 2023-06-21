using System;

namespace Riok.Mapperly.IntegrationTests.Models
{
    [Flags]
    public enum TestFlagsEnum
    {
        V1 = 1 << 0,
        V2 = 1 << 1,
        V4 = 1 << 2,
    }
}
