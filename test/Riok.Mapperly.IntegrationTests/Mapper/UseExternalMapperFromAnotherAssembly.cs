using System;
using System.Linq;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.TestDependency.Mapper;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    [UseStaticMapper(typeof(DateTimeMapper))]
    public static partial class UseExternalMapperFromAnotherAssembly
    {
        public static partial IQueryable<Target> MapToTarget(IQueryable<Source> source);

        public static partial Target MapToTarget(Source source);

        public class Source
        {
            public DateTime DateTime { get; set; }
        }

        public class Target
        {
            public DateTimeOffset DateTime { get; set; }
        }
    }
}
