using System.Collections.Generic;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper(UseDeepCloning = true)]
    public static partial class StackDeepCloningMapper
    {
        public static partial Stack<int> Copy(Stack<int> src);
    }

    [Mapper(UseDeepCloning = true, StackCloningStrategy = StackCloningStrategy.ReverseOrder)]
    public static partial class StackDeepCloningLegacyMapper
    {
        public static partial Stack<int> Copy(Stack<int> src);
    }
}
