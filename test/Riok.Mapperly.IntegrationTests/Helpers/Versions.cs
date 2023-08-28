using System;

namespace Riok.Mapperly.IntegrationTests.Helpers
{
    [Flags]
    public enum Versions
    {
        NETFRAMEWORK4_8 = 1 << 0,
        NET6_0 = 1 << 1,
        NET7_0 = 1 << 2,
    }
}
