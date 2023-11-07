#if !NET6_0_OR_GREATER
using System;
using System.Globalization;
using VerifyTests;

namespace Riok.Mapperly.IntegrationTests.Helpers
{
    /// <summary>
    /// Portable <see cref="DateOnly"/> converter for VerifyTests to ensure consistent json output across different net versions.
    /// Is necessary to handle net framework tests which include <see cref="DateOnly"/> per nuget package.
    /// </summary>
    public class PortableDateOnlyConverter : WriteOnlyJsonConverter<DateOnly>
    {
        public override void Write(VerifyJsonWriter writer, DateOnly value) =>
            // copied format from https://github.com/VerifyTests/Verify/blob/19.7.1/src/Verify/Serialization/VerifierSettings.cs#L58
            writer.WriteRawValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}
#endif
