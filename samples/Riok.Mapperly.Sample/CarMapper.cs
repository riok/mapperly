using System.Text;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Sample;

// Enums of source and target have different numeric values -> use ByName strategy to map them
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class CarMapper
{
    [MapProperty(nameof(Car.Manufacturer), nameof(CarDto.Producer))] // Map property with a different name in the target type
    public static partial CarDto MapCarToDto(Car car);
}

[Mapper(UseDeepCloning = true, MapOnlyPrimitives = true)]
public static partial class SampleSubMapper
{
    public static partial SampleClassWithSubClass Clone(SampleClassWithSubClass car);
}

public class SampleClassWithSubClass
{
    public string StringTest { get; set; }
    public DateTime DateTimeTest { get; set; }
    public DateOnly DateOnlyTest { get; set; }
    public TimeOnly TimeOnlyTest { get; set; }

    public double DoubleTest { get; set; }
    public float FloatTest { get; set; }
    public decimal DecimalTest { get; set; }
    public int IntTest { get; set; }
    public long LongTest { get; set; }
    public short ShortTest { get; set; }
    public byte ByteTest { get; set; }
    public bool BoolTest { get; set; }
    public char CharTest { get; set; }
    public Guid GuidTest { get; set; }
    public TimeSpan TimeSpanTest { get; set; }
    public DateTimeOffset DateTimeOffsetTest { get; set; }
    public string[] StringArrayTest { get; set; }
    public List<string> StringListTest { get; set; }
    public DateTime[] DateTimeArrayTest { get; set; }
    public List<DateTime> DateTimeListTest { get; set; }
    public double[] DoubleArrayTest { get; set; }
    public List<double> DoubleListTest { get; set; }
    public float[] FloatArrayTest { get; set; }
    public List<float> FloatListTest { get; set; }
    public decimal[] DecimalArrayTest { get; set; }
    public List<decimal> DecimalListTest { get; set; }
    public int[] IntArrayTest { get; set; }
    public List<int> IntListTest { get; set; }
    public long[] LongArrayTest { get; set; }
    public List<long> LongListTest { get; set; }
    public short[] ShortArrayTest { get; set; }
    public List<short> ShortListTest { get; set; }
    public byte[] ByteArrayTest { get; set; }
    public List<byte> ByteListTest { get; set; }
    public bool[] BoolArrayTest { get; set; }
    public List<bool> BoolListTest { get; set; }
    public char[] CharArrayTest { get; set; }
    public List<char> CharListTest { get; set; }
    public Guid[] GuidArrayTest { get; set; }
    public List<Guid> GuidListTest { get; set; }
    public TimeSpan[] TimeSpanArrayTest { get; set; }
    public List<TimeSpan> TimeSpanListTest { get; set; }
    public DateTimeOffset[] DateTimeOffsetArrayTest { get; set; }
    public List<DateTimeOffset> DateTimeOffsetListTest { get; set; }
    public Dictionary<string, string> StringDictionaryTest { get; set; }
    public Dictionary<string, DateTime> DateTimeDictionaryTest { get; set; }
    public Dictionary<string, double> DoubleDictionaryTest { get; set; }
    public Dictionary<string, float> FloatDictionaryTest { get; set; }
    public Dictionary<string, SubClass> SubClassDictionaryTest { get; set; }
    public string? NullableStringTest { get; set; }
    public DateTime? NullableDateTimeTest { get; set; }
    public double? NullableDoubleTest { get; set; }
    public float? NullableFloatTest { get; set; }
    public decimal? NullableDecimalTest { get; set; }
    public int? NullableIntTest { get; set; }
    public long? NullableLongTest { get; set; }
    public short? NullableShortTest { get; set; }
    public byte? NullableByteTest { get; set; }
    public bool? NullableBoolTest { get; set; }
    public char? NullableCharTest { get; set; }
    public Guid? NullableGuidTest { get; set; }
    public TimeSpan? NullableTimeSpanTest { get; set; }
    public DateTimeOffset? NullableDateTimeOffsetTest { get; set; }

    public SubClass SubClass { get; set; }
}

public class SubClass
{
    public string StringTest { get; set; }
    public DateTime DateTimeTest { get; set; }
    public DateOnly DateOnlyTest { get; set; }
    public double DoubleTest { get; set; }
    public float FloatTest { get; set; }
    public decimal DecimalTest { get; set; }
    public int IntTest { get; set; }
    public long LongTest { get; set; }
    public short ShortTest { get; set; }
    public byte ByteTest { get; set; }
    public bool BoolTest { get; set; }
    public char CharTest { get; set; }
    public Guid GuidTest { get; set; }
    public TimeSpan TimeSpanTest { get; set; }
    public DateTimeOffset DateTimeOffsetTest { get; set; }
    public string[] StringArrayTest { get; set; }
    public List<string> StringListTest { get; set; }
    public DateTime[] DateTimeArrayTest { get; set; }
    public List<DateTime> DateTimeListTest { get; set; }
    public double[] DoubleArrayTest { get; set; }
    public List<double> DoubleListTest { get; set; }
    public float[] FloatArrayTest { get; set; }
    public List<float> FloatListTest { get; set; }
    public decimal[] DecimalArrayTest { get; set; }
    public List<decimal> DecimalListTest { get; set; }
    public int[] IntArrayTest { get; set; }
    public List<int> IntListTest { get; set; }
    public long[] LongArrayTest { get; set; }
    public List<long> LongListTest { get; set; }
    public short[] ShortArrayTest { get; set; }
    public List<short> ShortListTest { get; set; }
    public byte[] ByteArrayTest { get; set; }
    public List<byte> ByteListTest { get; set; }
    public bool[] BoolArrayTest { get; set; }
    public List<bool> BoolListTest { get; set; }
    public char[] CharArrayTest { get; set; }
    public List<char> CharListTest { get; set; }
    public Guid[] GuidArrayTest { get; set; }
    public List<Guid> GuidListTest { get; set; }
    public TimeSpan[] TimeSpanArrayTest { get; set; }
    public List<TimeSpan> TimeSpanListTest { get; set; }
    public DateTimeOffset[] DateTimeOffsetArrayTest { get; set; }
    public List<DateTimeOffset> DateTimeOffsetListTest { get; set; }
    public Dictionary<string, string> StringDictionaryTest { get; set; }
    public Dictionary<string, DateTime> DateTimeDictionaryTest { get; set; }
    public Dictionary<string, double> DoubleDictionaryTest { get; set; }
    public Dictionary<string, float> FloatDictionaryTest { get; set; }
    public string? NullableStringTest { get; set; }
    public DateTime? NullableDateTimeTest { get; set; }
    public double? NullableDoubleTest { get; set; }
    public float? NullableFloatTest { get; set; }
    public decimal? NullableDecimalTest { get; set; }
    public int? NullableIntTest { get; set; }
    public long? NullableLongTest { get; set; }
    public short? NullableShortTest { get; set; }
    public byte? NullableByteTest { get; set; }
    public bool? NullableBoolTest { get; set; }
    public char? NullableCharTest { get; set; }
    public Guid? NullableGuidTest { get; set; }
    public TimeSpan? NullableTimeSpanTest { get; set; }
    public DateTimeOffset? NullableDateTimeOffsetTest { get; set; }
}
