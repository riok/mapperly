#if !NET6_0_OR_GREATER
using System;
using System.Globalization;
using VerifyTests;


namespace Riok.Mapperly.IntegrationTests.Helpers
{
    /// <summary>
    /// Portable <see cref="TimeOnly"/> converter for VerifyTests to ensure consistent json output across different net versions.
    /// Is necessary to handle net framework tests which include <see cref="TimeOnly"/> per nuget package.
    /// </summary>
    public class PortableTimeOnlyConverter : WriteOnlyJsonConverter<TimeOnly>
    {
        public override void Write(VerifyJsonWriter writer, TimeOnly value) =>
            // copied format from https://github.com/VerifyTests/Verify/blob/19.7.1/src/Verify/Serialization/VerifierSettings.cs#L65
            writer.WriteRawValue(value.ToString("h:mm tt", CultureInfo.InvariantCulture));
    }
}
#endif
