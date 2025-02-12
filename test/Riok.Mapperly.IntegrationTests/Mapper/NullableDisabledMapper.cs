#nullable disable

using System.Diagnostics.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public static partial class NullableDisabledMapper
    {
        public static partial MyDto Map(this MyClass src);

#if NET7_0_OR_GREATER
        [return: NotNullIfNotNull(nameof(src))]
#elif NET5_0_OR_GREATER
        [return: NotNullIfNotNull("src")]
#endif
        private static MyNestedDto MapNested(this MyNestedClass src)
        {
            if (src == null)
                return null;

            return new MyNestedDto(src.V);
        }

        public class MyClass
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }

            public MyNestedClass Nested { get; set; }
        }

        public record MyNestedClass(int V);

        public class MyDto
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }

            public MyNestedDto Nested { get; set; }
        }

        public record MyNestedDto(int V);
    }
}
