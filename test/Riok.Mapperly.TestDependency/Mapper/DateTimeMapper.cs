using System;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.TestDependency.Mapper
{
    [Mapper]
    public static partial class DateTimeMapper
    {
        public static DateTimeOffset MapToDateTimeOffset(DateTime dateTime) => new(dateTime, TimeSpan.Zero);
    }
}
