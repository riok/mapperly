using System;

namespace Riok.Mapperly.IntegrationTests.Helpers
{
    [Flags]
    public enum Versions
    {
        NETFRAMEWORK4_8 = 1 << 0,
        NET8_0 = 1 << 3,
        NET9_0 = 1 << 4,
        NET10_0 = 1 << 5,
    }
}
