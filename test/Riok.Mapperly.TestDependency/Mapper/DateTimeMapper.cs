using System;

namespace Riok.Mapperly.TestDependency.Mapper
{
    public static class DateTimeMapper
    {
        public static DateTimeOffset MapToDateTimeOffset(DateTime dateTime) => new(dateTime, TimeSpan.Zero);
    }
}
