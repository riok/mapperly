using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Diagnostics;

namespace Riok.Mapperly.Tests.Mapping;

public class StaticMethodConversionTest
{
    [
        Theory,
        InlineData("byte", "class  A { public static A Create(byte source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "struct A { public static A Create(byte source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "class  A { public static A CreateFrom(byte source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("byte", "struct A { public static A CreateFrom(byte source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "byte",
            "class  A { public static A CreateFromByte(byte source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData(
            "byte",
            "struct A { public static A CreateFromByte(byte source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData("byte", "class  A { public static A FromByte(byte source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("byte", "struct A { public static A FromByte(byte source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("sbyte", "class  A { public static A Create(sbyte source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "struct A { public static A Create(sbyte source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "class  A { public static A CreateFrom(sbyte source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("sbyte", "struct A { public static A CreateFrom(sbyte source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSByte(sbyte source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSByte(sbyte source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSbyte(sbyte source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSbyte(sbyte source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData("sbyte", "class  A { public static A FromSbyte(sbyte source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte", "struct A { public static A FromSbyte(sbyte source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte", "class  A { public static A FromSByte(sbyte source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("sbyte", "struct A { public static A FromSByte(sbyte source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("short", "class  A { public static A Create(short source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "struct A { public static A Create(short source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "class  A { public static A CreateFrom(short source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("short", "struct A { public static A CreateFrom(short source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "short",
            "class  A { public static A CreateFromShort(short source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromShort(short source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "class  A { public static A CreateFromInt16(short source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromInt16(short source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData("short", "class  A { public static A FromShort(short source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short", "struct A { public static A FromShort(short source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short", "class  A { public static A FromInt16(short source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("short", "struct A { public static A FromInt16(short source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("ushort", "class  A { public static A Create(ushort source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort", "struct A { public static A Create(ushort source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort", "class  A { public static A CreateFrom(ushort source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ushort", "struct A { public static A CreateFrom(ushort source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUShort(ushort source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUShort(ushort source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUshort(ushort source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUshort(ushort source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUInt16(ushort source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUInt16(ushort source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData("ushort", "class  A { public static A FromUShort(ushort source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort", "struct A { public static A FromUShort(ushort source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort", "class  A { public static A FromUshort(ushort source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort", "struct A { public static A FromUshort(ushort source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort", "class  A { public static A FromUInt16(ushort source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("ushort", "struct A { public static A FromUInt16(ushort source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("int", "class  A { public static A Create(int source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "struct A { public static A Create(int source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "class  A { public static A CreateFrom(int source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int", "struct A { public static A CreateFrom(int source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int", "class  A { public static A CreateFromInt(int source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData("int", "struct A { public static A CreateFromInt(int source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData(
            "int",
            "class  A { public static A CreateFromInt32(int source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData(
            "int",
            "struct A { public static A CreateFromInt32(int source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData("int", "class  A { public static A FromInt(int source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "struct A { public static A FromInt(int source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "class  A { public static A FromInt32(int source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("int", "struct A { public static A FromInt32(int source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("uint", "class  A { public static A Create(uint source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "struct A { public static A Create(uint source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "class  A { public static A CreateFrom(uint source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("uint", "struct A { public static A CreateFrom(uint source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt(uint source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt(uint source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUint(uint source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUint(uint source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt32(uint source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt32(uint source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData("uint", "class  A { public static A FromUInt(uint source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint", "struct A { public static A FromUInt(uint source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint", "class  A { public static A FromUint(uint source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint", "struct A { public static A FromUint(uint source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint", "class  A { public static A FromUInt32(uint source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("uint", "struct A { public static A FromUInt32(uint source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("long", "class  A { public static A Create(long source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "struct A { public static A Create(long source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "class  A { public static A CreateFrom(long source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("long", "struct A { public static A CreateFrom(long source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "long",
            "class  A { public static A CreateFromLong(long source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromLong(long source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "class  A { public static A CreateFromInt64(long source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromInt64(long source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData("long", "class  A { public static A FromLong(long source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long", "struct A { public static A FromLong(long source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long", "class  A { public static A FromInt64(long source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("long", "struct A { public static A FromInt64(long source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("ulong", "class  A { public static A Create(ulong source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "struct A { public static A Create(ulong source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "class  A { public static A CreateFrom(ulong source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ulong", "struct A { public static A CreateFrom(ulong source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromULong(ulong source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromULong(ulong source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUlong(ulong source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUlong(ulong source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUInt64(ulong source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUInt64(ulong source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData("ulong", "class  A { public static A FromULong(ulong source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong", "struct A { public static A FromULong(ulong source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong", "class  A { public static A FromUlong(ulong source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong", "struct A { public static A FromUlong(ulong source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong", "class  A { public static A FromUInt64(ulong source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("ulong", "struct A { public static A FromUInt64(ulong source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("float", "class  A { public static A Create(float source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "struct A { public static A Create(float source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "class  A { public static A CreateFrom(float source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("float", "struct A { public static A CreateFrom(float source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "float",
            "class  A { public static A CreateFromFloat(float source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromFloat(float source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "class  A { public static A CreateFromSingle(float source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromSingle(float source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData("float", "class  A { public static A FromFloat(float source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float", "struct A { public static A FromFloat(float source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float", "class  A { public static A FromSingle(float source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("float", "struct A { public static A FromSingle(float source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("double", "class  A { public static A Create(double source) => new(); }", "return global::A.Create(source);"),
        InlineData("double", "struct A { public static A Create(double source) => new(); }", "return global::A.Create(source);"),
        InlineData("double", "class  A { public static A CreateFrom(double source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("double", "struct A { public static A CreateFrom(double source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "double",
            "class  A { public static A CreateFromDouble(double source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData(
            "double",
            "struct A { public static A CreateFromDouble(double source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData("double", "class  A { public static A FromDouble(double source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("double", "struct A { public static A FromDouble(double source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("char", "class  A { public static A Create(char source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "struct A { public static A Create(char source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "class  A { public static A CreateFrom(char source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("char", "struct A { public static A CreateFrom(char source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "char",
            "class  A { public static A CreateFromChar(char source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData(
            "char",
            "struct A { public static A CreateFromChar(char source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData("char", "class  A { public static A FromChar(char source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("char", "struct A { public static A FromChar(char source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("bool", "class  A { public static A Create(bool source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(bool source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A CreateFrom(bool source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("bool", "struct A { public static A CreateFrom(bool source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBool(bool source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBool(bool source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBoolean(bool source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBoolean(bool source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData("bool", "class  A { public static A FromBool(bool source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool", "struct A { public static A FromBool(bool source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool", "class  A { public static A FromBoolean(bool source) => new(); }", "return global::A.FromBoolean(source);"),
        InlineData("bool", "struct A { public static A FromBoolean(bool source) => new(); }", "return global::A.FromBoolean(source);"),
    ]
    public void CustomTypeWithStaticFromSourceMethod(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("byte[]", "class  A { public static A Create(byte[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte[]", "struct A { public static A Create(byte[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte[]", "class  A { public static A CreateFrom(byte[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("byte[]", "struct A { public static A CreateFrom(byte[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "byte[]",
            "class  A { public static A CreateFromByteArray(byte[] source) => new(); }",
            "return global::A.CreateFromByteArray(source);"
        ),
        InlineData(
            "byte[]",
            "struct A { public static A CreateFromByteArray(byte[] source) => new(); }",
            "return global::A.CreateFromByteArray(source);"
        ),
        InlineData(
            "byte[]",
            "class  A { public static A FromByteArray(byte[] source) => new(); }",
            "return global::A.FromByteArray(source);"
        ),
        InlineData(
            "byte[]",
            "struct A { public static A FromByteArray(byte[] source) => new(); }",
            "return global::A.FromByteArray(source);"
        ),
        InlineData("sbyte[]", "class  A { public static A Create(sbyte[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte[]", "struct A { public static A Create(sbyte[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte[]", "class  A { public static A CreateFrom(sbyte[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("sbyte[]", "struct A { public static A CreateFrom(sbyte[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "sbyte[]",
            "class  A { public static A CreateFromSByteArray(sbyte[] source) => new(); }",
            "return global::A.CreateFromSByteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "struct A { public static A CreateFromSByteArray(sbyte[] source) => new(); }",
            "return global::A.CreateFromSByteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "class  A { public static A CreateFromSbyteArray(sbyte[] source) => new(); }",
            "return global::A.CreateFromSbyteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "struct A { public static A CreateFromSbyteArray(sbyte[] source) => new(); }",
            "return global::A.CreateFromSbyteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "class  A { public static A FromSbyteArray(sbyte[] source) => new(); }",
            "return global::A.FromSbyteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "struct A { public static A FromSbyteArray(sbyte[] source) => new(); }",
            "return global::A.FromSbyteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "class  A { public static A FromSByteArray(sbyte[] source) => new(); }",
            "return global::A.FromSByteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "struct A { public static A FromSByteArray(sbyte[] source) => new(); }",
            "return global::A.FromSByteArray(source);"
        ),
        InlineData("short[]", "class  A { public static A Create(short[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("short[]", "struct A { public static A Create(short[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("short[]", "class  A { public static A CreateFrom(short[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("short[]", "struct A { public static A CreateFrom(short[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "short[]",
            "class  A { public static A CreateFromShortArray(short[] source) => new(); }",
            "return global::A.CreateFromShortArray(source);"
        ),
        InlineData(
            "short[]",
            "struct A { public static A CreateFromShortArray(short[] source) => new(); }",
            "return global::A.CreateFromShortArray(source);"
        ),
        InlineData(
            "short[]",
            "class  A { public static A CreateFromInt16Array(short[] source) => new(); }",
            "return global::A.CreateFromInt16Array(source);"
        ),
        InlineData(
            "short[]",
            "struct A { public static A CreateFromInt16Array(short[] source) => new(); }",
            "return global::A.CreateFromInt16Array(source);"
        ),
        InlineData(
            "short[]",
            "class  A { public static A FromShortArray(short[] source) => new(); }",
            "return global::A.FromShortArray(source);"
        ),
        InlineData(
            "short[]",
            "struct A { public static A FromShortArray(short[] source) => new(); }",
            "return global::A.FromShortArray(source);"
        ),
        InlineData(
            "short[]",
            "class  A { public static A FromInt16Array(short[] source) => new(); }",
            "return global::A.FromInt16Array(source);"
        ),
        InlineData(
            "short[]",
            "struct A { public static A FromInt16Array(short[] source) => new(); }",
            "return global::A.FromInt16Array(source);"
        ),
        InlineData("ushort[]", "class  A { public static A Create(ushort[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort[]", "struct A { public static A Create(ushort[] source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "ushort[]",
            "class  A { public static A CreateFrom(ushort[] source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A CreateFrom(ushort[] source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static A CreateFromUShortArray(ushort[] source) => new(); }",
            "return global::A.CreateFromUShortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A CreateFromUShortArray(ushort[] source) => new(); }",
            "return global::A.CreateFromUShortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static A CreateFromUshortArray(ushort[] source) => new(); }",
            "return global::A.CreateFromUshortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A CreateFromUshortArray(ushort[] source) => new(); }",
            "return global::A.CreateFromUshortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static A CreateFromUInt16Array(ushort[] source) => new(); }",
            "return global::A.CreateFromUInt16Array(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A CreateFromUInt16Array(ushort[] source) => new(); }",
            "return global::A.CreateFromUInt16Array(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static A FromUShortArray(ushort[] source) => new(); }",
            "return global::A.FromUShortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A FromUShortArray(ushort[] source) => new(); }",
            "return global::A.FromUShortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static A FromUshortArray(ushort[] source) => new(); }",
            "return global::A.FromUshortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A FromUshortArray(ushort[] source) => new(); }",
            "return global::A.FromUshortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static A FromUInt16Array(ushort[] source) => new(); }",
            "return global::A.FromUInt16Array(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static A FromUInt16Array(ushort[] source) => new(); }",
            "return global::A.FromUInt16Array(source);"
        ),
        InlineData("int[]", "class  A { public static A Create(int[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("int[]", "struct A { public static A Create(int[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("int[]", "class  A { public static A CreateFrom(int[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int[]", "struct A { public static A CreateFrom(int[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "int[]",
            "class  A { public static A CreateFromIntArray(int[] source) => new(); }",
            "return global::A.CreateFromIntArray(source);"
        ),
        InlineData(
            "int[]",
            "struct A { public static A CreateFromIntArray(int[] source) => new(); }",
            "return global::A.CreateFromIntArray(source);"
        ),
        InlineData(
            "int[]",
            "class  A { public static A CreateFromInt32Array(int[] source) => new(); }",
            "return global::A.CreateFromInt32Array(source);"
        ),
        InlineData(
            "int[]",
            "struct A { public static A CreateFromInt32Array(int[] source) => new(); }",
            "return global::A.CreateFromInt32Array(source);"
        ),
        InlineData("int[]", "class  A { public static A FromIntArray(int[] source) => new(); }", "return global::A.FromIntArray(source);"),
        InlineData("int[]", "struct A { public static A FromIntArray(int[] source) => new(); }", "return global::A.FromIntArray(source);"),
        InlineData(
            "int[]",
            "class  A { public static A FromInt32Array(int[] source) => new(); }",
            "return global::A.FromInt32Array(source);"
        ),
        InlineData(
            "int[]",
            "struct A { public static A FromInt32Array(int[] source) => new(); }",
            "return global::A.FromInt32Array(source);"
        ),
        InlineData("uint[]", "class  A { public static A Create(uint[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint[]", "struct A { public static A Create(uint[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint[]", "class  A { public static A CreateFrom(uint[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("uint[]", "struct A { public static A CreateFrom(uint[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "uint[]",
            "class  A { public static A CreateFromUIntArray(uint[] source) => new(); }",
            "return global::A.CreateFromUIntArray(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static A CreateFromUIntArray(uint[] source) => new(); }",
            "return global::A.CreateFromUIntArray(source);"
        ),
        InlineData(
            "uint[]",
            "class  A { public static A CreateFromUintArray(uint[] source) => new(); }",
            "return global::A.CreateFromUintArray(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static A CreateFromUintArray(uint[] source) => new(); }",
            "return global::A.CreateFromUintArray(source);"
        ),
        InlineData(
            "uint[]",
            "class  A { public static A CreateFromUInt32Array(uint[] source) => new(); }",
            "return global::A.CreateFromUInt32Array(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static A CreateFromUInt32Array(uint[] source) => new(); }",
            "return global::A.CreateFromUInt32Array(source);"
        ),
        InlineData(
            "uint[]",
            "class  A { public static A FromUIntArray(uint[] source) => new(); }",
            "return global::A.FromUIntArray(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static A FromUIntArray(uint[] source) => new(); }",
            "return global::A.FromUIntArray(source);"
        ),
        InlineData(
            "uint[]",
            "class  A { public static A FromUintArray(uint[] source) => new(); }",
            "return global::A.FromUintArray(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static A FromUintArray(uint[] source) => new(); }",
            "return global::A.FromUintArray(source);"
        ),
        InlineData(
            "uint[]",
            "class  A { public static A FromUInt32Array(uint[] source) => new(); }",
            "return global::A.FromUInt32Array(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static A FromUInt32Array(uint[] source) => new(); }",
            "return global::A.FromUInt32Array(source);"
        ),
        InlineData("long[]", "class  A { public static A Create(long[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("long[]", "struct A { public static A Create(long[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("long[]", "class  A { public static A CreateFrom(long[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("long[]", "struct A { public static A CreateFrom(long[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "long[]",
            "class  A { public static A CreateFromLongArray(long[] source) => new(); }",
            "return global::A.CreateFromLongArray(source);"
        ),
        InlineData(
            "long[]",
            "struct A { public static A CreateFromLongArray(long[] source) => new(); }",
            "return global::A.CreateFromLongArray(source);"
        ),
        InlineData(
            "long[]",
            "class  A { public static A CreateFromInt64Array(long[] source) => new(); }",
            "return global::A.CreateFromInt64Array(source);"
        ),
        InlineData(
            "long[]",
            "struct A { public static A CreateFromInt64Array(long[] source) => new(); }",
            "return global::A.CreateFromInt64Array(source);"
        ),
        InlineData(
            "long[]",
            "class  A { public static A FromLongArray(long[] source) => new(); }",
            "return global::A.FromLongArray(source);"
        ),
        InlineData(
            "long[]",
            "struct A { public static A FromLongArray(long[] source) => new(); }",
            "return global::A.FromLongArray(source);"
        ),
        InlineData(
            "long[]",
            "class  A { public static A FromInt64Array(long[] source) => new(); }",
            "return global::A.FromInt64Array(source);"
        ),
        InlineData(
            "long[]",
            "struct A { public static A FromInt64Array(long[] source) => new(); }",
            "return global::A.FromInt64Array(source);"
        ),
        InlineData("ulong[]", "class  A { public static A Create(ulong[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong[]", "struct A { public static A Create(ulong[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong[]", "class  A { public static A CreateFrom(ulong[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ulong[]", "struct A { public static A CreateFrom(ulong[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ulong[]",
            "class  A { public static A CreateFromULongArray(ulong[] source) => new(); }",
            "return global::A.CreateFromULongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static A CreateFromULongArray(ulong[] source) => new(); }",
            "return global::A.CreateFromULongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "class  A { public static A CreateFromUlongArray(ulong[] source) => new(); }",
            "return global::A.CreateFromUlongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static A CreateFromUlongArray(ulong[] source) => new(); }",
            "return global::A.CreateFromUlongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "class  A { public static A CreateFromUInt64Array(ulong[] source) => new(); }",
            "return global::A.CreateFromUInt64Array(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static A CreateFromUInt64Array(ulong[] source) => new(); }",
            "return global::A.CreateFromUInt64Array(source);"
        ),
        InlineData(
            "ulong[]",
            "class  A { public static A FromULongArray(ulong[] source) => new(); }",
            "return global::A.FromULongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static A FromULongArray(ulong[] source) => new(); }",
            "return global::A.FromULongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "class  A { public static A FromUlongArray(ulong[] source) => new(); }",
            "return global::A.FromUlongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static A FromUlongArray(ulong[] source) => new(); }",
            "return global::A.FromUlongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "class  A { public static A FromUInt64Array(ulong[] source) => new(); }",
            "return global::A.FromUInt64Array(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static A FromUInt64Array(ulong[] source) => new(); }",
            "return global::A.FromUInt64Array(source);"
        ),
        InlineData("float[]", "class  A { public static A Create(float[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("float[]", "struct A { public static A Create(float[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("float[]", "class  A { public static A CreateFrom(float[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("float[]", "struct A { public static A CreateFrom(float[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "float[]",
            "class  A { public static A CreateFromFloatArray(float[] source) => new(); }",
            "return global::A.CreateFromFloatArray(source);"
        ),
        InlineData(
            "float[]",
            "struct A { public static A CreateFromFloatArray(float[] source) => new(); }",
            "return global::A.CreateFromFloatArray(source);"
        ),
        InlineData(
            "float[]",
            "class  A { public static A CreateFromSingleArray(float[] source) => new(); }",
            "return global::A.CreateFromSingleArray(source);"
        ),
        InlineData(
            "float[]",
            "struct A { public static A CreateFromSingleArray(float[] source) => new(); }",
            "return global::A.CreateFromSingleArray(source);"
        ),
        InlineData(
            "float[]",
            "class  A { public static A FromFloatArray(float[] source) => new(); }",
            "return global::A.FromFloatArray(source);"
        ),
        InlineData(
            "float[]",
            "struct A { public static A FromFloatArray(float[] source) => new(); }",
            "return global::A.FromFloatArray(source);"
        ),
        InlineData(
            "float[]",
            "class  A { public static A FromSingleArray(float[] source) => new(); }",
            "return global::A.FromSingleArray(source);"
        ),
        InlineData(
            "float[]",
            "struct A { public static A FromSingleArray(float[] source) => new(); }",
            "return global::A.FromSingleArray(source);"
        ),
        InlineData("double[]", "class  A { public static A Create(double[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("double[]", "struct A { public static A Create(double[] source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "double[]",
            "class  A { public static A CreateFrom(double[] source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "double[]",
            "struct A { public static A CreateFrom(double[] source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "double[]",
            "class  A { public static A CreateFromDoubleArray(double[] source) => new(); }",
            "return global::A.CreateFromDoubleArray(source);"
        ),
        InlineData(
            "double[]",
            "struct A { public static A CreateFromDoubleArray(double[] source) => new(); }",
            "return global::A.CreateFromDoubleArray(source);"
        ),
        InlineData(
            "double[]",
            "class  A { public static A FromDoubleArray(double[] source) => new(); }",
            "return global::A.FromDoubleArray(source);"
        ),
        InlineData(
            "double[]",
            "struct A { public static A FromDoubleArray(double[] source) => new(); }",
            "return global::A.FromDoubleArray(source);"
        ),
        InlineData("char[]", "class  A { public static A Create(char[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("char[]", "struct A { public static A Create(char[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("char[]", "class  A { public static A CreateFrom(char[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("char[]", "struct A { public static A CreateFrom(char[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "char[]",
            "class  A { public static A CreateFromCharArray(char[] source) => new(); }",
            "return global::A.CreateFromCharArray(source);"
        ),
        InlineData(
            "char[]",
            "struct A { public static A CreateFromCharArray(char[] source) => new(); }",
            "return global::A.CreateFromCharArray(source);"
        ),
        InlineData(
            "char[]",
            "class  A { public static A FromCharArray(char[] source) => new(); }",
            "return global::A.FromCharArray(source);"
        ),
        InlineData(
            "char[]",
            "struct A { public static A FromCharArray(char[] source) => new(); }",
            "return global::A.FromCharArray(source);"
        ),
        InlineData("bool[]", "class  A { public static A Create(bool[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool[]", "struct A { public static A Create(bool[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool[]", "class  A { public static A CreateFrom(bool[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("bool[]", "struct A { public static A CreateFrom(bool[] source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "bool[]",
            "class  A { public static A CreateFromBoolArray(bool[] source) => new(); }",
            "return global::A.CreateFromBoolArray(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A CreateFromBoolArray(bool[] source) => new(); }",
            "return global::A.CreateFromBoolArray(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A CreateFromBooleanArray(bool[] source) => new(); }",
            "return global::A.CreateFromBooleanArray(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A CreateFromBooleanArray(bool[] source) => new(); }",
            "return global::A.CreateFromBooleanArray(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A FromBoolArray(bool[] source) => new(); }",
            "return global::A.FromBoolArray(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A FromBoolArray(bool[] source) => new(); }",
            "return global::A.FromBoolArray(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A FromBooleanArray(bool[] source) => new(); }",
            "return global::A.FromBooleanArray(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A FromBooleanArray(bool[] source) => new(); }",
            "return global::A.FromBooleanArray(source);"
        ),
    ]
    public void CustomTypeWithStaticFromSourceArrayMethod(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("byte", "class  A { public static A Create(byte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "struct A { public static A Create(byte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "class  A { public static A CreateFrom(byte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("byte", "struct A { public static A CreateFrom(byte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "byte",
            "class  A { public static A CreateFromByte(byte? source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData(
            "byte",
            "struct A { public static A CreateFromByte(byte? source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData("byte", "class  A { public static A FromByte(byte? source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("byte", "struct A { public static A FromByte(byte? source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("sbyte", "class  A { public static A Create(sbyte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "struct A { public static A Create(sbyte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "class  A { public static A CreateFrom(sbyte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("sbyte", "struct A { public static A CreateFrom(sbyte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSByte(sbyte? source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSByte(sbyte? source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSbyte(sbyte? source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSbyte(sbyte? source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData("sbyte", "class  A { public static A FromSbyte(sbyte? source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte", "struct A { public static A FromSbyte(sbyte? source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte", "class  A { public static A FromSByte(sbyte? source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("sbyte", "struct A { public static A FromSByte(sbyte? source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("short", "class  A { public static A Create(short? source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "struct A { public static A Create(short? source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "class  A { public static A CreateFrom(short? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("short", "struct A { public static A CreateFrom(short? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "short",
            "class  A { public static A CreateFromShort(short? source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromShort(short? source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "class  A { public static A CreateFromInt16(short? source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromInt16(short? source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData("short", "class  A { public static A FromShort(short? source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short", "struct A { public static A FromShort(short? source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short", "class  A { public static A FromInt16(short? source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("short", "struct A { public static A FromInt16(short? source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("ushort", "class  A { public static A Create(ushort? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort", "struct A { public static A Create(ushort? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort", "class  A { public static A CreateFrom(ushort? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ushort", "struct A { public static A CreateFrom(ushort? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUShort(ushort? source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUShort(ushort? source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUshort(ushort? source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUshort(ushort? source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUInt16(ushort? source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUInt16(ushort? source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData("ushort", "class  A { public static A FromUShort(ushort? source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort", "struct A { public static A FromUShort(ushort? source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort", "class  A { public static A FromUshort(ushort? source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort", "struct A { public static A FromUshort(ushort? source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort", "class  A { public static A FromUInt16(ushort? source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("ushort", "struct A { public static A FromUInt16(ushort? source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("int", "class  A { public static A Create(int? source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "struct A { public static A Create(int? source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "class  A { public static A CreateFrom(int? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int", "struct A { public static A CreateFrom(int? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int", "class  A { public static A CreateFromInt(int? source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData("int", "struct A { public static A CreateFromInt(int? source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData(
            "int",
            "class  A { public static A CreateFromInt32(int? source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData(
            "int",
            "struct A { public static A CreateFromInt32(int? source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData("int", "class  A { public static A FromInt(int? source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "struct A { public static A FromInt(int? source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "class  A { public static A FromInt32(int? source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("int", "struct A { public static A FromInt32(int? source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("uint", "class  A { public static A Create(uint? source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "struct A { public static A Create(uint? source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "class  A { public static A CreateFrom(uint? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("uint", "struct A { public static A CreateFrom(uint? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt(uint? source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt(uint? source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUint(uint? source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUint(uint? source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt32(uint? source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt32(uint? source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData("uint", "class  A { public static A FromUInt(uint? source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint", "struct A { public static A FromUInt(uint? source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint", "class  A { public static A FromUint(uint? source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint", "struct A { public static A FromUint(uint? source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint", "class  A { public static A FromUInt32(uint? source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("uint", "struct A { public static A FromUInt32(uint? source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("long", "class  A { public static A Create(long? source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "struct A { public static A Create(long? source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "class  A { public static A CreateFrom(long? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("long", "struct A { public static A CreateFrom(long? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "long",
            "class  A { public static A CreateFromLong(long? source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromLong(long? source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "class  A { public static A CreateFromInt64(long? source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromInt64(long? source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData("long", "class  A { public static A FromLong(long? source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long", "struct A { public static A FromLong(long? source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long", "class  A { public static A FromInt64(long? source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("long", "struct A { public static A FromInt64(long? source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("ulong", "class  A { public static A Create(ulong? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "struct A { public static A Create(ulong? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "class  A { public static A CreateFrom(ulong? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ulong", "struct A { public static A CreateFrom(ulong? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromULong(ulong? source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromULong(ulong? source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUlong(ulong? source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUlong(ulong? source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUInt64(ulong? source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUInt64(ulong? source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData("ulong", "class  A { public static A FromULong(ulong? source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong", "struct A { public static A FromULong(ulong? source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong", "class  A { public static A FromUlong(ulong? source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong", "struct A { public static A FromUlong(ulong? source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong", "class  A { public static A FromUInt64(ulong? source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("ulong", "struct A { public static A FromUInt64(ulong? source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("float", "class  A { public static A Create(float? source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "struct A { public static A Create(float? source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "class  A { public static A CreateFrom(float? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("float", "struct A { public static A CreateFrom(float? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "float",
            "class  A { public static A CreateFromFloat(float? source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromFloat(float? source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "class  A { public static A CreateFromSingle(float? source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromSingle(float? source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData("float", "class  A { public static A FromFloat(float? source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float", "struct A { public static A FromFloat(float? source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float", "class  A { public static A FromSingle(float? source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("float", "struct A { public static A FromSingle(float? source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("double", "class  A { public static A Create(double? source) => new(); }", "return global::A.Create(source);"),
        InlineData("double", "struct A { public static A Create(double? source) => new(); }", "return global::A.Create(source);"),
        InlineData("double", "class  A { public static A CreateFrom(double? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("double", "struct A { public static A CreateFrom(double? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "double",
            "class  A { public static A CreateFromDouble(double? source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData(
            "double",
            "struct A { public static A CreateFromDouble(double? source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData("double", "class  A { public static A FromDouble(double? source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("double", "struct A { public static A FromDouble(double? source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("char", "class  A { public static A Create(char? source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "struct A { public static A Create(char? source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "class  A { public static A CreateFrom(char? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("char", "struct A { public static A CreateFrom(char? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "char",
            "class  A { public static A CreateFromChar(char? source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData(
            "char",
            "struct A { public static A CreateFromChar(char? source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData("char", "class  A { public static A FromChar(char? source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("char", "struct A { public static A FromChar(char? source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("bool", "class  A { public static A Create(bool? source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(bool? source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A CreateFrom(bool? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("bool", "struct A { public static A CreateFrom(bool? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBool(bool? source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBool(bool? source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBoolean(bool? source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBoolean(bool? source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData("bool", "class  A { public static A FromBool(bool? source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool", "struct A { public static A FromBool(bool? source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool", "class  A { public static A FromBoolean(bool? source) => new(); }", "return global::A.FromBoolean(source);"),
        InlineData("bool", "struct A { public static A FromBoolean(bool? source) => new(); }", "return global::A.FromBoolean(source);"),
    ]
    public void CustomTypeWithStaticFromSourceMethodWithNullableParameter(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory(Skip = "Not work, RMG090: Mapping the nullable source of type ... to target of type A which is not nullable"),
        InlineData("byte?", "class  A { public static A Create(byte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte?", "struct A { public static A Create(byte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte?", "class  A { public static A CreateFrom(byte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("byte?", "struct A { public static A CreateFrom(byte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "byte?",
            "class  A { public static A CreateFromByte(byte? source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData(
            "byte?",
            "struct A { public static A CreateFromByte(byte? source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData("byte?", "class  A { public static A FromByte(byte? source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("byte?", "struct A { public static A FromByte(byte? source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("sbyte?", "class  A { public static A Create(sbyte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte?", "struct A { public static A Create(sbyte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte?", "class  A { public static A CreateFrom(sbyte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("sbyte?", "struct A { public static A CreateFrom(sbyte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "sbyte?",
            "class  A { public static A CreateFromSByte(sbyte? source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte?",
            "struct A { public static A CreateFromSByte(sbyte? source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte?",
            "class  A { public static A CreateFromSbyte(sbyte? source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData(
            "sbyte?",
            "struct A { public static A CreateFromSbyte(sbyte? source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData("sbyte?", "class  A { public static A FromSbyte(sbyte? source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte?", "struct A { public static A FromSbyte(sbyte? source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte?", "class  A { public static A FromSByte(sbyte? source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("sbyte?", "struct A { public static A FromSByte(sbyte? source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("short?", "class  A { public static A Create(short? source) => new(); }", "return global::A.Create(source);"),
        InlineData("short?", "struct A { public static A Create(short? source) => new(); }", "return global::A.Create(source);"),
        InlineData("short?", "class  A { public static A CreateFrom(short? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("short?", "struct A { public static A CreateFrom(short? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "short?",
            "class  A { public static A CreateFromShort(short? source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short?",
            "struct A { public static A CreateFromShort(short? source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short?",
            "class  A { public static A CreateFromInt16(short? source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData(
            "short?",
            "struct A { public static A CreateFromInt16(short? source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData("short?", "class  A { public static A FromShort(short? source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short?", "struct A { public static A FromShort(short? source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short?", "class  A { public static A FromInt16(short? source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("short?", "struct A { public static A FromInt16(short? source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("ushort?", "class  A { public static A Create(ushort? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort?", "struct A { public static A Create(ushort? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort?", "class  A { public static A CreateFrom(ushort? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ushort?", "struct A { public static A CreateFrom(ushort? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ushort?",
            "class  A { public static A CreateFromUShort(ushort? source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort?",
            "struct A { public static A CreateFromUShort(ushort? source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort?",
            "class  A { public static A CreateFromUshort(ushort? source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort?",
            "struct A { public static A CreateFromUshort(ushort? source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort?",
            "class  A { public static A CreateFromUInt16(ushort? source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData(
            "ushort?",
            "struct A { public static A CreateFromUInt16(ushort? source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData("ushort?", "class  A { public static A FromUShort(ushort? source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort?", "struct A { public static A FromUShort(ushort? source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort?", "class  A { public static A FromUshort(ushort? source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort?", "struct A { public static A FromUshort(ushort? source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort?", "class  A { public static A FromUInt16(ushort? source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("ushort?", "struct A { public static A FromUInt16(ushort? source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("int?", "class  A { public static A Create(int? source) => new(); }", "return global::A.Create(source);"),
        InlineData("int?", "struct A { public static A Create(int? source) => new(); }", "return global::A.Create(source);"),
        InlineData("int?", "class  A { public static A CreateFrom(int? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int?", "struct A { public static A CreateFrom(int? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int?", "class  A { public static A CreateFromInt(int? source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData("int?", "struct A { public static A CreateFromInt(int? source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData(
            "int?",
            "class  A { public static A CreateFromInt32(int? source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData(
            "int?",
            "struct A { public static A CreateFromInt32(int? source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData("int?", "class  A { public static A FromInt(int? source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int?", "struct A { public static A FromInt(int? source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int?", "class  A { public static A FromInt32(int? source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("int?", "struct A { public static A FromInt32(int? source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("uint?", "class  A { public static A Create(uint? source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint?", "struct A { public static A Create(uint? source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint?", "class  A { public static A CreateFrom(uint? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("uint?", "struct A { public static A CreateFrom(uint? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "uint?",
            "class  A { public static A CreateFromUInt(uint? source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint?",
            "struct A { public static A CreateFromUInt(uint? source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint?",
            "class  A { public static A CreateFromUint(uint? source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint?",
            "struct A { public static A CreateFromUint(uint? source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint?",
            "class  A { public static A CreateFromUInt32(uint? source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData(
            "uint?",
            "struct A { public static A CreateFromUInt32(uint? source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData("uint?", "class  A { public static A FromUInt(uint? source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint?", "struct A { public static A FromUInt(uint? source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint?", "class  A { public static A FromUint(uint? source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint?", "struct A { public static A FromUint(uint? source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint?", "class  A { public static A FromUInt32(uint? source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("uint?", "struct A { public static A FromUInt32(uint? source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("long?", "class  A { public static A Create(long? source) => new(); }", "return global::A.Create(source);"),
        InlineData("long?", "struct A { public static A Create(long? source) => new(); }", "return global::A.Create(source);"),
        InlineData("long?", "class  A { public static A CreateFrom(long? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("long?", "struct A { public static A CreateFrom(long? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "long?",
            "class  A { public static A CreateFromLong(long? source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long?",
            "struct A { public static A CreateFromLong(long? source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long?",
            "class  A { public static A CreateFromInt64(long? source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData(
            "long?",
            "struct A { public static A CreateFromInt64(long? source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData("long?", "class  A { public static A FromLong(long? source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long?", "struct A { public static A FromLong(long? source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long?", "class  A { public static A FromInt64(long? source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("long?", "struct A { public static A FromInt64(long? source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("ulong?", "class  A { public static A Create(ulong? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong?", "struct A { public static A Create(ulong? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong?", "class  A { public static A CreateFrom(ulong? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ulong?", "struct A { public static A CreateFrom(ulong? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ulong?",
            "class  A { public static A CreateFromULong(ulong? source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong?",
            "struct A { public static A CreateFromULong(ulong? source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong?",
            "class  A { public static A CreateFromUlong(ulong? source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong?",
            "struct A { public static A CreateFromUlong(ulong? source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong?",
            "class  A { public static A CreateFromUInt64(ulong? source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData(
            "ulong?",
            "struct A { public static A CreateFromUInt64(ulong? source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData("ulong?", "class  A { public static A FromULong(ulong? source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong?", "struct A { public static A FromULong(ulong? source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong?", "class  A { public static A FromUlong(ulong? source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong?", "struct A { public static A FromUlong(ulong? source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong?", "class  A { public static A FromUInt64(ulong? source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("ulong?", "struct A { public static A FromUInt64(ulong? source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("float?", "class  A { public static A Create(float? source) => new(); }", "return global::A.Create(source);"),
        InlineData("float?", "struct A { public static A Create(float? source) => new(); }", "return global::A.Create(source);"),
        InlineData("float?", "class  A { public static A CreateFrom(float? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("float?", "struct A { public static A CreateFrom(float? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "float?",
            "class  A { public static A CreateFromFloat(float? source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float?",
            "struct A { public static A CreateFromFloat(float? source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float?",
            "class  A { public static A CreateFromSingle(float? source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData(
            "float?",
            "struct A { public static A CreateFromSingle(float? source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData("float?", "class  A { public static A FromFloat(float? source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float?", "struct A { public static A FromFloat(float? source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float?", "class  A { public static A FromSingle(float? source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("float?", "struct A { public static A FromSingle(float? source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("double?", "class  A { public static A Create(double? source) => new(); }", "return global::A.Create(source);"),
        InlineData("double?", "struct A { public static A Create(double? source) => new(); }", "return global::A.Create(source);"),
        InlineData("double?", "class  A { public static A CreateFrom(double? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("double?", "struct A { public static A CreateFrom(double? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "double?",
            "class  A { public static A CreateFromDouble(double? source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData(
            "double?",
            "struct A { public static A CreateFromDouble(double? source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData("double?", "class  A { public static A FromDouble(double? source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("double?", "struct A { public static A FromDouble(double? source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("char?", "class  A { public static A Create(char? source) => new(); }", "return global::A.Create(source);"),
        InlineData("char?", "struct A { public static A Create(char? source) => new(); }", "return global::A.Create(source);"),
        InlineData("char?", "class  A { public static A CreateFrom(char? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("char?", "struct A { public static A CreateFrom(char? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "char?",
            "class  A { public static A CreateFromChar(char? source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData(
            "char?",
            "struct A { public static A CreateFromChar(char? source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData("char?", "class  A { public static A FromChar(char? source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("char?", "struct A { public static A FromChar(char? source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("bool?", "class  A { public static A Create(bool? source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool?", "struct A { public static A Create(bool? source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool?", "class  A { public static A CreateFrom(bool? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("bool?", "struct A { public static A CreateFrom(bool? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "bool?",
            "class  A { public static A CreateFromBool(bool? source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool?",
            "struct A { public static A CreateFromBool(bool? source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool?",
            "class  A { public static A CreateFromBoolean(bool? source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData(
            "bool?",
            "struct A { public static A CreateFromBoolean(bool? source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData("bool?", "class  A { public static A FromBool(bool? source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool?", "struct A { public static A FromBool(bool? source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool?", "class  A { public static A FromBoolean(bool? source) => new(); }", "return global::A.FromBoolean(source);"),
        InlineData("bool?", "struct A { public static A FromBoolean(bool? source) => new(); }", "return global::A.FromBoolean(source);"),
    ]
    public void CustomTypeWithStaticFromSourceMethodWithNullableParameterAndNullableSource(
        string sourceType,
        string classDecl,
        string expectedResult
    )
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A?", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("byte", "class  A { public static A Create(byte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "struct A { public static A Create(byte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "class  A { public static A CreateFrom(byte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("byte", "struct A { public static A CreateFrom(byte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "byte",
            "class  A { public static A CreateFromByte(byte? source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData(
            "byte",
            "struct A { public static A CreateFromByte(byte? source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData("byte", "class  A { public static A FromByte(byte? source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("byte", "struct A { public static A FromByte(byte? source) => new(); }", "return global::A.FromByte(source);"),
        InlineData("sbyte", "class  A { public static A Create(sbyte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "struct A { public static A Create(sbyte? source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "class  A { public static A CreateFrom(sbyte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("sbyte", "struct A { public static A CreateFrom(sbyte? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSByte(sbyte? source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSByte(sbyte? source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSbyte(sbyte? source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSbyte(sbyte? source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData("sbyte", "class  A { public static A FromSbyte(sbyte? source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte", "struct A { public static A FromSbyte(sbyte? source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData("sbyte", "class  A { public static A FromSByte(sbyte? source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("sbyte", "struct A { public static A FromSByte(sbyte? source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData("short", "class  A { public static A Create(short? source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "struct A { public static A Create(short? source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "class  A { public static A CreateFrom(short? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("short", "struct A { public static A CreateFrom(short? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "short",
            "class  A { public static A CreateFromShort(short? source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromShort(short? source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "class  A { public static A CreateFromInt16(short? source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromInt16(short? source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData("short", "class  A { public static A FromShort(short? source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short", "struct A { public static A FromShort(short? source) => new(); }", "return global::A.FromShort(source);"),
        InlineData("short", "class  A { public static A FromInt16(short? source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("short", "struct A { public static A FromInt16(short? source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData("ushort", "class  A { public static A Create(ushort? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort", "struct A { public static A Create(ushort? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ushort", "class  A { public static A CreateFrom(ushort? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ushort", "struct A { public static A CreateFrom(ushort? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUShort(ushort? source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUShort(ushort? source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUshort(ushort? source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUshort(ushort? source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUInt16(ushort? source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUInt16(ushort? source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData("ushort", "class  A { public static A FromUShort(ushort? source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort", "struct A { public static A FromUShort(ushort? source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData("ushort", "class  A { public static A FromUshort(ushort? source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort", "struct A { public static A FromUshort(ushort? source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData("ushort", "class  A { public static A FromUInt16(ushort? source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("ushort", "struct A { public static A FromUInt16(ushort? source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData("int", "class  A { public static A Create(int? source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "struct A { public static A Create(int? source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "class  A { public static A CreateFrom(int? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int", "struct A { public static A CreateFrom(int? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("int", "class  A { public static A CreateFromInt(int? source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData("int", "struct A { public static A CreateFromInt(int? source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData(
            "int",
            "class  A { public static A CreateFromInt32(int? source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData(
            "int",
            "struct A { public static A CreateFromInt32(int? source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData("int", "class  A { public static A FromInt(int? source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "struct A { public static A FromInt(int? source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "class  A { public static A FromInt32(int? source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("int", "struct A { public static A FromInt32(int? source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData("uint", "class  A { public static A Create(uint? source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "struct A { public static A Create(uint? source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "class  A { public static A CreateFrom(uint? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("uint", "struct A { public static A CreateFrom(uint? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt(uint? source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt(uint? source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUint(uint? source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUint(uint? source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt32(uint? source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt32(uint? source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData("uint", "class  A { public static A FromUInt(uint? source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint", "struct A { public static A FromUInt(uint? source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData("uint", "class  A { public static A FromUint(uint? source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint", "struct A { public static A FromUint(uint? source) => new(); }", "return global::A.FromUint(source);"),
        InlineData("uint", "class  A { public static A FromUInt32(uint? source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("uint", "struct A { public static A FromUInt32(uint? source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData("long", "class  A { public static A Create(long? source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "struct A { public static A Create(long? source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "class  A { public static A CreateFrom(long? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("long", "struct A { public static A CreateFrom(long? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "long",
            "class  A { public static A CreateFromLong(long? source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromLong(long? source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "class  A { public static A CreateFromInt64(long? source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromInt64(long? source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData("long", "class  A { public static A FromLong(long? source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long", "struct A { public static A FromLong(long? source) => new(); }", "return global::A.FromLong(source);"),
        InlineData("long", "class  A { public static A FromInt64(long? source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("long", "struct A { public static A FromInt64(long? source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData("ulong", "class  A { public static A Create(ulong? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "struct A { public static A Create(ulong? source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "class  A { public static A CreateFrom(ulong? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("ulong", "struct A { public static A CreateFrom(ulong? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromULong(ulong? source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromULong(ulong? source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUlong(ulong? source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUlong(ulong? source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUInt64(ulong? source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUInt64(ulong? source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData("ulong", "class  A { public static A FromULong(ulong? source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong", "struct A { public static A FromULong(ulong? source) => new(); }", "return global::A.FromULong(source);"),
        InlineData("ulong", "class  A { public static A FromUlong(ulong? source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong", "struct A { public static A FromUlong(ulong? source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData("ulong", "class  A { public static A FromUInt64(ulong? source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("ulong", "struct A { public static A FromUInt64(ulong? source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData("float", "class  A { public static A Create(float? source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "struct A { public static A Create(float? source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "class  A { public static A CreateFrom(float? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("float", "struct A { public static A CreateFrom(float? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "float",
            "class  A { public static A CreateFromFloat(float? source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromFloat(float? source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "class  A { public static A CreateFromSingle(float? source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromSingle(float? source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData("float", "class  A { public static A FromFloat(float? source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float", "struct A { public static A FromFloat(float? source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData("float", "class  A { public static A FromSingle(float? source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("float", "struct A { public static A FromSingle(float? source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData("double", "class  A { public static A Create(double? source) => new(); }", "return global::A.Create(source);"),
        InlineData("double", "struct A { public static A Create(double? source) => new(); }", "return global::A.Create(source);"),
        InlineData("double", "class  A { public static A CreateFrom(double? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("double", "struct A { public static A CreateFrom(double? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "double",
            "class  A { public static A CreateFromDouble(double? source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData(
            "double",
            "struct A { public static A CreateFromDouble(double? source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData("double", "class  A { public static A FromDouble(double? source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("double", "struct A { public static A FromDouble(double? source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData("char", "class  A { public static A Create(char? source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "struct A { public static A Create(char? source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "class  A { public static A CreateFrom(char? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("char", "struct A { public static A CreateFrom(char? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "char",
            "class  A { public static A CreateFromChar(char? source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData(
            "char",
            "struct A { public static A CreateFromChar(char? source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData("char", "class  A { public static A FromChar(char? source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("char", "struct A { public static A FromChar(char? source) => new(); }", "return global::A.FromChar(source);"),
        InlineData("bool", "class  A { public static A Create(bool? source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(bool? source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A CreateFrom(bool? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData("bool", "struct A { public static A CreateFrom(bool? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBool(bool? source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBool(bool? source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBoolean(bool? source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBoolean(bool? source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData("bool", "class  A { public static A FromBool(bool? source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool", "struct A { public static A FromBool(bool? source) => new(); }", "return global::A.FromBool(source);"),
        InlineData("bool", "class  A { public static A FromBoolean(bool? source) => new(); }", "return global::A.FromBoolean(source);"),
        InlineData("bool", "struct A { public static A FromBoolean(bool? source) => new(); }", "return global::A.FromBoolean(source);"),
    ]
    public void CustomTypeWithStaticFromSourceMethodWithNullableTargetAndNullableParameter(
        string sourceType,
        string classDecl,
        string expectedResult
    )
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A?", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("byte", "class  A { public static A Create(byte source) => new(); }", "return global::A.Create(source);"),
        InlineData("byte", "struct A { public static A Create(byte source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("byte", "class  A { public static A CreateFrom(byte source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "byte",
            "struct A { public static A CreateFrom(byte source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "byte",
            "class  A { public static A CreateFromByte(byte source) => new(); }",
            "return global::A.CreateFromByte(source);"
        ),
        InlineData(
            "byte",
            "struct A { public static A CreateFromByte(byte source) => new(); }",
            "return (global::A?)global::A.CreateFromByte(source);"
        ),
        InlineData("byte", "class  A { public static A FromByte(byte source) => new(); }", "return global::A.FromByte(source);"),
        InlineData(
            "byte",
            "struct A { public static A FromByte(byte source) => new(); }",
            "return (global::A?)global::A.FromByte(source);"
        ),
        InlineData("sbyte", "class  A { public static A Create(sbyte source) => new(); }", "return global::A.Create(source);"),
        InlineData("sbyte", "struct A { public static A Create(sbyte source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("sbyte", "class  A { public static A CreateFrom(sbyte source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFrom(sbyte source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSByte(sbyte source) => new(); }",
            "return global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSByte(sbyte source) => new(); }",
            "return (global::A?)global::A.CreateFromSByte(source);"
        ),
        InlineData(
            "sbyte",
            "class  A { public static A CreateFromSbyte(sbyte source) => new(); }",
            "return global::A.CreateFromSbyte(source);"
        ),
        InlineData(
            "sbyte",
            "struct A { public static A CreateFromSbyte(sbyte source) => new(); }",
            "return (global::A?)global::A.CreateFromSbyte(source);"
        ),
        InlineData("sbyte", "class  A { public static A FromSbyte(sbyte source) => new(); }", "return global::A.FromSbyte(source);"),
        InlineData(
            "sbyte",
            "struct A { public static A FromSbyte(sbyte source) => new(); }",
            "return (global::A?)global::A.FromSbyte(source);"
        ),
        InlineData("sbyte", "class  A { public static A FromSByte(sbyte source) => new(); }", "return global::A.FromSByte(source);"),
        InlineData(
            "sbyte",
            "struct A { public static A FromSByte(sbyte source) => new(); }",
            "return (global::A?)global::A.FromSByte(source);"
        ),
        InlineData("short", "class  A { public static A Create(short source) => new(); }", "return global::A.Create(source);"),
        InlineData("short", "struct A { public static A Create(short source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("short", "class  A { public static A CreateFrom(short source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "short",
            "struct A { public static A CreateFrom(short source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "short",
            "class  A { public static A CreateFromShort(short source) => new(); }",
            "return global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromShort(short source) => new(); }",
            "return (global::A?)global::A.CreateFromShort(source);"
        ),
        InlineData(
            "short",
            "class  A { public static A CreateFromInt16(short source) => new(); }",
            "return global::A.CreateFromInt16(source);"
        ),
        InlineData(
            "short",
            "struct A { public static A CreateFromInt16(short source) => new(); }",
            "return (global::A?)global::A.CreateFromInt16(source);"
        ),
        InlineData("short", "class  A { public static A FromShort(short source) => new(); }", "return global::A.FromShort(source);"),
        InlineData(
            "short",
            "struct A { public static A FromShort(short source) => new(); }",
            "return (global::A?)global::A.FromShort(source);"
        ),
        InlineData("short", "class  A { public static A FromInt16(short source) => new(); }", "return global::A.FromInt16(source);"),
        InlineData(
            "short",
            "struct A { public static A FromInt16(short source) => new(); }",
            "return (global::A?)global::A.FromInt16(source);"
        ),
        InlineData("ushort", "class  A { public static A Create(ushort source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "ushort",
            "struct A { public static A Create(ushort source) => new(); }",
            "return (global::A?)global::A.Create(source);"
        ),
        InlineData("ushort", "class  A { public static A CreateFrom(ushort source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ushort",
            "struct A { public static A CreateFrom(ushort source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUShort(ushort source) => new(); }",
            "return global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUShort(ushort source) => new(); }",
            "return (global::A?)global::A.CreateFromUShort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUshort(ushort source) => new(); }",
            "return global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUshort(ushort source) => new(); }",
            "return (global::A?)global::A.CreateFromUshort(source);"
        ),
        InlineData(
            "ushort",
            "class  A { public static A CreateFromUInt16(ushort source) => new(); }",
            "return global::A.CreateFromUInt16(source);"
        ),
        InlineData(
            "ushort",
            "struct A { public static A CreateFromUInt16(ushort source) => new(); }",
            "return (global::A?)global::A.CreateFromUInt16(source);"
        ),
        InlineData("ushort", "class  A { public static A FromUShort(ushort source) => new(); }", "return global::A.FromUShort(source);"),
        InlineData(
            "ushort",
            "struct A { public static A FromUShort(ushort source) => new(); }",
            "return (global::A?)global::A.FromUShort(source);"
        ),
        InlineData("ushort", "class  A { public static A FromUshort(ushort source) => new(); }", "return global::A.FromUshort(source);"),
        InlineData(
            "ushort",
            "struct A { public static A FromUshort(ushort source) => new(); }",
            "return (global::A?)global::A.FromUshort(source);"
        ),
        InlineData("ushort", "class  A { public static A FromUInt16(ushort source) => new(); }", "return global::A.FromUInt16(source);"),
        InlineData(
            "ushort",
            "struct A { public static A FromUInt16(ushort source) => new(); }",
            "return (global::A?)global::A.FromUInt16(source);"
        ),
        InlineData("int", "class  A { public static A Create(int source) => new(); }", "return global::A.Create(source);"),
        InlineData("int", "struct A { public static A Create(int source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("int", "class  A { public static A CreateFrom(int source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "int",
            "struct A { public static A CreateFrom(int source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData("int", "class  A { public static A CreateFromInt(int source) => new(); }", "return global::A.CreateFromInt(source);"),
        InlineData(
            "int",
            "struct A { public static A CreateFromInt(int source) => new(); }",
            "return (global::A?)global::A.CreateFromInt(source);"
        ),
        InlineData(
            "int",
            "class  A { public static A CreateFromInt32(int source) => new(); }",
            "return global::A.CreateFromInt32(source);"
        ),
        InlineData(
            "int",
            "struct A { public static A CreateFromInt32(int source) => new(); }",
            "return (global::A?)global::A.CreateFromInt32(source);"
        ),
        InlineData("int", "class  A { public static A FromInt(int source) => new(); }", "return global::A.FromInt(source);"),
        InlineData("int", "struct A { public static A FromInt(int source) => new(); }", "return (global::A?)global::A.FromInt(source);"),
        InlineData("int", "class  A { public static A FromInt32(int source) => new(); }", "return global::A.FromInt32(source);"),
        InlineData(
            "int",
            "struct A { public static A FromInt32(int source) => new(); }",
            "return (global::A?)global::A.FromInt32(source);"
        ),
        InlineData("uint", "class  A { public static A Create(uint source) => new(); }", "return global::A.Create(source);"),
        InlineData("uint", "struct A { public static A Create(uint source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("uint", "class  A { public static A CreateFrom(uint source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "uint",
            "struct A { public static A CreateFrom(uint source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt(uint source) => new(); }",
            "return global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt(uint source) => new(); }",
            "return (global::A?)global::A.CreateFromUInt(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUint(uint source) => new(); }",
            "return global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUint(uint source) => new(); }",
            "return (global::A?)global::A.CreateFromUint(source);"
        ),
        InlineData(
            "uint",
            "class  A { public static A CreateFromUInt32(uint source) => new(); }",
            "return global::A.CreateFromUInt32(source);"
        ),
        InlineData(
            "uint",
            "struct A { public static A CreateFromUInt32(uint source) => new(); }",
            "return (global::A?)global::A.CreateFromUInt32(source);"
        ),
        InlineData("uint", "class  A { public static A FromUInt(uint source) => new(); }", "return global::A.FromUInt(source);"),
        InlineData(
            "uint",
            "struct A { public static A FromUInt(uint source) => new(); }",
            "return (global::A?)global::A.FromUInt(source);"
        ),
        InlineData("uint", "class  A { public static A FromUint(uint source) => new(); }", "return global::A.FromUint(source);"),
        InlineData(
            "uint",
            "struct A { public static A FromUint(uint source) => new(); }",
            "return (global::A?)global::A.FromUint(source);"
        ),
        InlineData("uint", "class  A { public static A FromUInt32(uint source) => new(); }", "return global::A.FromUInt32(source);"),
        InlineData(
            "uint",
            "struct A { public static A FromUInt32(uint source) => new(); }",
            "return (global::A?)global::A.FromUInt32(source);"
        ),
        InlineData("long", "class  A { public static A Create(long source) => new(); }", "return global::A.Create(source);"),
        InlineData("long", "struct A { public static A Create(long source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("long", "class  A { public static A CreateFrom(long source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "long",
            "struct A { public static A CreateFrom(long source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "long",
            "class  A { public static A CreateFromLong(long source) => new(); }",
            "return global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromLong(long source) => new(); }",
            "return (global::A?)global::A.CreateFromLong(source);"
        ),
        InlineData(
            "long",
            "class  A { public static A CreateFromInt64(long source) => new(); }",
            "return global::A.CreateFromInt64(source);"
        ),
        InlineData(
            "long",
            "struct A { public static A CreateFromInt64(long source) => new(); }",
            "return (global::A?)global::A.CreateFromInt64(source);"
        ),
        InlineData("long", "class  A { public static A FromLong(long source) => new(); }", "return global::A.FromLong(source);"),
        InlineData(
            "long",
            "struct A { public static A FromLong(long source) => new(); }",
            "return (global::A?)global::A.FromLong(source);"
        ),
        InlineData("long", "class  A { public static A FromInt64(long source) => new(); }", "return global::A.FromInt64(source);"),
        InlineData(
            "long",
            "struct A { public static A FromInt64(long source) => new(); }",
            "return (global::A?)global::A.FromInt64(source);"
        ),
        InlineData("ulong", "class  A { public static A Create(ulong source) => new(); }", "return global::A.Create(source);"),
        InlineData("ulong", "struct A { public static A Create(ulong source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("ulong", "class  A { public static A CreateFrom(ulong source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "ulong",
            "struct A { public static A CreateFrom(ulong source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromULong(ulong source) => new(); }",
            "return global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromULong(ulong source) => new(); }",
            "return (global::A?)global::A.CreateFromULong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUlong(ulong source) => new(); }",
            "return global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUlong(ulong source) => new(); }",
            "return (global::A?)global::A.CreateFromUlong(source);"
        ),
        InlineData(
            "ulong",
            "class  A { public static A CreateFromUInt64(ulong source) => new(); }",
            "return global::A.CreateFromUInt64(source);"
        ),
        InlineData(
            "ulong",
            "struct A { public static A CreateFromUInt64(ulong source) => new(); }",
            "return (global::A?)global::A.CreateFromUInt64(source);"
        ),
        InlineData("ulong", "class  A { public static A FromULong(ulong source) => new(); }", "return global::A.FromULong(source);"),
        InlineData(
            "ulong",
            "struct A { public static A FromULong(ulong source) => new(); }",
            "return (global::A?)global::A.FromULong(source);"
        ),
        InlineData("ulong", "class  A { public static A FromUlong(ulong source) => new(); }", "return global::A.FromUlong(source);"),
        InlineData(
            "ulong",
            "struct A { public static A FromUlong(ulong source) => new(); }",
            "return (global::A?)global::A.FromUlong(source);"
        ),
        InlineData("ulong", "class  A { public static A FromUInt64(ulong source) => new(); }", "return global::A.FromUInt64(source);"),
        InlineData(
            "ulong",
            "struct A { public static A FromUInt64(ulong source) => new(); }",
            "return (global::A?)global::A.FromUInt64(source);"
        ),
        InlineData("float", "class  A { public static A Create(float source) => new(); }", "return global::A.Create(source);"),
        InlineData("float", "struct A { public static A Create(float source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("float", "class  A { public static A CreateFrom(float source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "float",
            "struct A { public static A CreateFrom(float source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "float",
            "class  A { public static A CreateFromFloat(float source) => new(); }",
            "return global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromFloat(float source) => new(); }",
            "return (global::A?)global::A.CreateFromFloat(source);"
        ),
        InlineData(
            "float",
            "class  A { public static A CreateFromSingle(float source) => new(); }",
            "return global::A.CreateFromSingle(source);"
        ),
        InlineData(
            "float",
            "struct A { public static A CreateFromSingle(float source) => new(); }",
            "return (global::A?)global::A.CreateFromSingle(source);"
        ),
        InlineData("float", "class  A { public static A FromFloat(float source) => new(); }", "return global::A.FromFloat(source);"),
        InlineData(
            "float",
            "struct A { public static A FromFloat(float source) => new(); }",
            "return (global::A?)global::A.FromFloat(source);"
        ),
        InlineData("float", "class  A { public static A FromSingle(float source) => new(); }", "return global::A.FromSingle(source);"),
        InlineData(
            "float",
            "struct A { public static A FromSingle(float source) => new(); }",
            "return (global::A?)global::A.FromSingle(source);"
        ),
        InlineData("double", "class  A { public static A Create(double source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "double",
            "struct A { public static A Create(double source) => new(); }",
            "return (global::A?)global::A.Create(source);"
        ),
        InlineData("double", "class  A { public static A CreateFrom(double source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "double",
            "struct A { public static A CreateFrom(double source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "double",
            "class  A { public static A CreateFromDouble(double source) => new(); }",
            "return global::A.CreateFromDouble(source);"
        ),
        InlineData(
            "double",
            "struct A { public static A CreateFromDouble(double source) => new(); }",
            "return (global::A?)global::A.CreateFromDouble(source);"
        ),
        InlineData("double", "class  A { public static A FromDouble(double source) => new(); }", "return global::A.FromDouble(source);"),
        InlineData(
            "double",
            "struct A { public static A FromDouble(double source) => new(); }",
            "return (global::A?)global::A.FromDouble(source);"
        ),
        InlineData("char", "class  A { public static A Create(char source) => new(); }", "return global::A.Create(source);"),
        InlineData("char", "struct A { public static A Create(char source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("char", "class  A { public static A CreateFrom(char source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "char",
            "struct A { public static A CreateFrom(char source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "char",
            "class  A { public static A CreateFromChar(char source) => new(); }",
            "return global::A.CreateFromChar(source);"
        ),
        InlineData(
            "char",
            "struct A { public static A CreateFromChar(char source) => new(); }",
            "return (global::A?)global::A.CreateFromChar(source);"
        ),
        InlineData("char", "class  A { public static A FromChar(char source) => new(); }", "return global::A.FromChar(source);"),
        InlineData(
            "char",
            "struct A { public static A FromChar(char source) => new(); }",
            "return (global::A?)global::A.FromChar(source);"
        ),
        InlineData("bool", "class  A { public static A Create(bool source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(bool source) => new(); }", "return (global::A?)global::A.Create(source);"),
        InlineData("bool", "class  A { public static A CreateFrom(bool source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "bool",
            "struct A { public static A CreateFrom(bool source) => new(); }",
            "return (global::A?)global::A.CreateFrom(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBool(bool source) => new(); }",
            "return global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBool(bool source) => new(); }",
            "return (global::A?)global::A.CreateFromBool(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A CreateFromBoolean(bool source) => new(); }",
            "return global::A.CreateFromBoolean(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A CreateFromBoolean(bool source) => new(); }",
            "return (global::A?)global::A.CreateFromBoolean(source);"
        ),
        InlineData("bool", "class  A { public static A FromBool(bool source) => new(); }", "return global::A.FromBool(source);"),
        InlineData(
            "bool",
            "struct A { public static A FromBool(bool source) => new(); }",
            "return (global::A?)global::A.FromBool(source);"
        ),
        InlineData("bool", "class  A { public static A FromBoolean(bool source) => new(); }", "return global::A.FromBoolean(source);"),
        InlineData(
            "bool",
            "struct A { public static A FromBoolean(bool source) => new(); }",
            "return (global::A?)global::A.FromBoolean(source);"
        ),
    ]
    public void CustomTypeWithStaticFromSourceMethodWithNullableTarget(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A?", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("bool", "class  A { public static A Create(params bool[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params bool[] source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "bool",
            "class  A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A Create(params ICollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params ICollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData("bool", "class  A { public static A Create(params IList<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params IList<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A Create(params List<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params List<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A Create(params Span<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params Span<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "bool",
            "class  A { public static A Create(params ReadOnlySpan<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params ReadOnlySpan<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData("bool[]", "class  A { public static A Create(params bool[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool[]", "struct A { public static A Create(params bool[] source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "bool[]",
            "class  A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A Create(params ICollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A Create(params ICollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "class  A { public static A Create(params IList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A Create(params IList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData("bool[]", "class  A { public static A Create(params Span<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool[]", "struct A { public static A Create(params Span<bool> source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "bool[]",
            "class  A { public static A Create(params ReadOnlySpan<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static A Create(params ReadOnlySpan<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "class  A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "struct A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "class  A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "struct A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "class  A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "struct A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "class  A { public static A Create(params ICollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "struct A { public static A Create(params ICollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "class  A { public static A Create(params IList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "List<bool>",
            "struct A { public static A Create(params IList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "IReadOnlyList<bool>",
            "class  A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "IReadOnlyList<bool>",
            "struct A { public static A Create(params IEnumerable<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "IReadOnlyList<bool>",
            "class  A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "IReadOnlyList<bool>",
            "struct A { public static A Create(params IReadOnlyCollection<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "IReadOnlyList<bool>",
            "class  A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "IReadOnlyList<bool>",
            "struct A { public static A Create(params IReadOnlyList<bool> source) => new(); }",
            "return global::A.Create(source);"
        ),
    ]
    public void CustomTypeWithStaticFromSourceMethodWithParamsArgument(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("bool", "class  A { public static A Create(params bool?[] source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params bool?[] source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "bool",
            "class  A { public static A Create(params IEnumerable<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params IEnumerable<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A Create(params IReadOnlyCollection<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params IReadOnlyCollection<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A Create(params IReadOnlyList<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params IReadOnlyList<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "class  A { public static A Create(params ICollection<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params ICollection<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData("bool", "class  A { public static A Create(params IList<bool?> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params IList<bool?> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A Create(params List<bool?> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params List<bool?> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "class  A { public static A Create(params Span<bool?> source) => new(); }", "return global::A.Create(source);"),
        InlineData("bool", "struct A { public static A Create(params Span<bool?> source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "bool",
            "class  A { public static A Create(params ReadOnlySpan<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "bool",
            "struct A { public static A Create(params ReadOnlySpan<bool?> source) => new(); }",
            "return global::A.Create(source);"
        ),
    ]
    public void CustomTypeWithStaticFromSourceMethodWithParamsNullableArgument(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("DateTime", "class  A { public static A Create(DateTime source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "DateTime",
            "struct A { public static A CreateFrom(DateTime source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "DateTime",
            "class A { public static A CreateFromDateTime(DateTime source) => new(); }",
            "return global::A.CreateFromDateTime(source);"
        ),
        InlineData(
            "DateTime",
            "struct A { public static A FromDateTime(DateTime source) => new(); }",
            "return global::A.FromDateTime(source);"
        ),
        InlineData("DateTime", "class  A { public static A Create(DateTime? source) => new(); }", "return global::A.Create(source);"),
        InlineData(
            "DateTime",
            "struct A { public static A CreateFrom(DateTime? source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "DateTime",
            "class A { public static A CreateFromDateTime(DateTime? source) => new(); }",
            "return global::A.CreateFromDateTime(source);"
        ),
        InlineData(
            "DateTime",
            "struct A { public static A FromDateTime(DateTime? source) => new(); }",
            "return global::A.FromDateTime(source);"
        ),
        InlineData(
            "(int A, int B)",
            "class  A { public static A Create((int A, int B) source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "(int A, int B)",
            "struct A { public static A CreateFrom((int A, int B) source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData(
            "(int A, int B)",
            "class  A { public static A Create((int A, int B)? source) => new(); }",
            "return global::A.Create(source);"
        ),
        InlineData(
            "(int A, int B)",
            "struct A { public static A CreateFrom((int A, int B)? source) => new(); }",
            "return global::A.CreateFrom(source);"
        ),
        InlineData("Version", "class  A { public static A Create(Version source) => new(); }", "return global::A.Create(source);"),
        InlineData("Version", "struct A { public static A CreateFrom(Version source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "Version",
            "class  A { public static A CreateFromVersion(Version source) => new(); }",
            "return global::A.CreateFromVersion(source);"
        ),
        InlineData(
            "Version",
            "struct A { public static A FromVersion(Version source) => new(); }",
            "return global::A.FromVersion(source);"
        ),
        InlineData("Version", "class  A { public static A Create(Version? source) => new(); }", "return global::A.Create(source);"),
        InlineData("Version", "struct A { public static A CreateFrom(Version? source) => new(); }", "return global::A.CreateFrom(source);"),
        InlineData(
            "Version",
            "class  A { public static A CreateFromVersion(Version? source) => new(); }",
            "return global::A.CreateFromVersion(source);"
        ),
        InlineData(
            "Version",
            "struct A { public static A FromVersion(Version? source) => new(); }",
            "return global::A.FromVersion(source);"
        ),
    ]
    public void CustomTypeWithStaticFromSourceMethodWhereArgumentIsComplexType(string sourceType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping(sourceType, "A", classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [
        Theory,
        InlineData("byte", "class  A { public static byte ToByte(A source) => new(); }", "return global::A.ToByte(source);"),
        InlineData("byte", "struct A { public static byte ToByte(A source) => new(); }", "return global::A.ToByte(source);"),
        InlineData("byte[]", "class  A { public static byte[] ToByteArray(A source) => new(); }", "return global::A.ToByteArray(source);"),
        InlineData("byte[]", "struct A { public static byte[] ToByteArray(A source) => new(); }", "return global::A.ToByteArray(source);"),
        InlineData("sbyte", "class  A { public static sbyte ToSByte(A source) => new(); }", "return global::A.ToSByte(source);"),
        InlineData("sbyte", "struct A { public static sbyte ToSByte(A source) => new(); }", "return global::A.ToSByte(source);"),
        InlineData(
            "sbyte[]",
            "class  A { public static sbyte[] ToSByteArray(A source) => new(); }",
            "return global::A.ToSByteArray(source);"
        ),
        InlineData(
            "sbyte[]",
            "struct A { public static sbyte[] ToSByteArray(A source) => new(); }",
            "return global::A.ToSByteArray(source);"
        ),
        InlineData("short", "class  A { public static short ToShort(A source) => new(); }", "return global::A.ToShort(source);"),
        InlineData("short", "struct A { public static short ToShort(A source) => new(); }", "return global::A.ToShort(source);"),
        InlineData("short", "class  A { public static short ToInt16(A source) => new(); }", "return global::A.ToInt16(source);"),
        InlineData("short", "struct A { public static short ToInt16(A source) => new(); }", "return global::A.ToInt16(source);"),
        InlineData(
            "short[]",
            "class  A { public static short[] ToShortArray(A source) => new(); }",
            "return global::A.ToShortArray(source);"
        ),
        InlineData(
            "short[]",
            "struct A { public static short[] ToShortArray(A source) => new(); }",
            "return global::A.ToShortArray(source);"
        ),
        InlineData(
            "short[]",
            "class  A { public static short[] ToInt16Array(A source) => new(); }",
            "return global::A.ToInt16Array(source);"
        ),
        InlineData(
            "short[]",
            "struct A { public static short[] ToInt16Array(A source) => new(); }",
            "return global::A.ToInt16Array(source);"
        ),
        InlineData("ushort", "class  A { public static ushort ToUShort(A source) => new(); }", "return global::A.ToUShort(source);"),
        InlineData("ushort", "struct A { public static ushort ToUShort(A source) => new(); }", "return global::A.ToUShort(source);"),
        InlineData("ushort", "class  A { public static ushort ToUInt16(A source) => new(); }", "return global::A.ToUInt16(source);"),
        InlineData("ushort", "struct A { public static ushort ToUInt16(A source) => new(); }", "return global::A.ToUInt16(source);"),
        InlineData(
            "ushort[]",
            "class  A { public static ushort[] ToUShortArray(A source) => new(); }",
            "return global::A.ToUShortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static ushort[] ToUShortArray(A source) => new(); }",
            "return global::A.ToUShortArray(source);"
        ),
        InlineData(
            "ushort[]",
            "class  A { public static ushort[] ToUInt16Array(A source) => new(); }",
            "return global::A.ToUInt16Array(source);"
        ),
        InlineData(
            "ushort[]",
            "struct A { public static ushort[] ToUInt16Array(A source) => new(); }",
            "return global::A.ToUInt16Array(source);"
        ),
        InlineData("int", "class  A { public static int ToInt(A source) => new(); }", "return global::A.ToInt(source);"),
        InlineData("int", "struct A { public static int ToInt(A source) => new(); }", "return global::A.ToInt(source);"),
        InlineData("int", "class  A { public static int ToInt32(A source) => new(); }", "return global::A.ToInt32(source);"),
        InlineData("int", "struct A { public static int ToInt32(A source) => new(); }", "return global::A.ToInt32(source);"),
        InlineData("int[]", "class  A { public static int[] ToIntArray(A source) => new(); }", "return global::A.ToIntArray(source);"),
        InlineData("int[]", "struct A { public static int[] ToIntArray(A source) => new(); }", "return global::A.ToIntArray(source);"),
        InlineData("int[]", "class  A { public static int[] ToInt32Array(A source) => new(); }", "return global::A.ToInt32Array(source);"),
        InlineData("int[]", "struct A { public static int[] ToInt32Array(A source) => new(); }", "return global::A.ToInt32Array(source);"),
        InlineData("uint", "class  A { public static uint ToUInt(A source) => new(); }", "return global::A.ToUInt(source);"),
        InlineData("uint", "struct A { public static uint ToUInt(A source) => new(); }", "return global::A.ToUInt(source);"),
        InlineData("uint", "class  A { public static uint ToUInt32(A source) => new(); }", "return global::A.ToUInt32(source);"),
        InlineData("uint", "struct A { public static uint ToUInt32(A source) => new(); }", "return global::A.ToUInt32(source);"),
        InlineData("uint[]", "class  A { public static uint[] ToUIntArray(A source) => new(); }", "return global::A.ToUIntArray(source);"),
        InlineData("uint[]", "struct A { public static uint[] ToUIntArray(A source) => new(); }", "return global::A.ToUIntArray(source);"),
        InlineData(
            "uint[]",
            "class  A { public static uint[] ToUInt32Array(A source) => new(); }",
            "return global::A.ToUInt32Array(source);"
        ),
        InlineData(
            "uint[]",
            "struct A { public static uint[] ToUInt32Array(A source) => new(); }",
            "return global::A.ToUInt32Array(source);"
        ),
        InlineData("long", "class  A { public static long ToLong(A source) => new(); }", "return global::A.ToLong(source);"),
        InlineData("long", "struct A { public static long ToLong(A source) => new(); }", "return global::A.ToLong(source);"),
        InlineData("long", "class  A { public static long ToInt64(A source) => new(); }", "return global::A.ToInt64(source);"),
        InlineData("long", "struct A { public static long ToInt64(A source) => new(); }", "return global::A.ToInt64(source);"),
        InlineData("long[]", "class  A { public static long[] ToLongArray(A source) => new(); }", "return global::A.ToLongArray(source);"),
        InlineData("long[]", "struct A { public static long[] ToLongArray(A source) => new(); }", "return global::A.ToLongArray(source);"),
        InlineData(
            "long[]",
            "class  A { public static long[] ToInt64Array(A source) => new(); }",
            "return global::A.ToInt64Array(source);"
        ),
        InlineData(
            "long[]",
            "struct A { public static long[] ToInt64Array(A source) => new(); }",
            "return global::A.ToInt64Array(source);"
        ),
        InlineData("ulong", "class  A { public static ulong ToULong(A source) => new(); }", "return global::A.ToULong(source);"),
        InlineData("ulong", "struct A { public static ulong ToULong(A source) => new(); }", "return global::A.ToULong(source);"),
        InlineData("ulong", "class  A { public static ulong ToUInt64(A source) => new(); }", "return global::A.ToUInt64(source);"),
        InlineData("ulong", "struct A { public static ulong ToUInt64(A source) => new(); }", "return global::A.ToUInt64(source);"),
        InlineData(
            "ulong[]",
            "class  A { public static ulong[] ToULongArray(A source) => new(); }",
            "return global::A.ToULongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static ulong[] ToULongArray(A source) => new(); }",
            "return global::A.ToULongArray(source);"
        ),
        InlineData(
            "ulong[]",
            "class  A { public static ulong[] ToUInt64Array(A source) => new(); }",
            "return global::A.ToUInt64Array(source);"
        ),
        InlineData(
            "ulong[]",
            "struct A { public static ulong[] ToUInt64Array(A source) => new(); }",
            "return global::A.ToUInt64Array(source);"
        ),
        InlineData("float", "class  A { public static float ToFloat(A source) => new(); }", "return global::A.ToFloat(source);"),
        InlineData("float", "struct A { public static float ToFloat(A source) => new(); }", "return global::A.ToFloat(source);"),
        InlineData("float", "class  A { public static float ToSingle(A source) => new(); }", "return global::A.ToSingle(source);"),
        InlineData("float", "struct A { public static float ToSingle(A source) => new(); }", "return global::A.ToSingle(source);"),
        InlineData(
            "float[]",
            "class  A { public static float[] ToFloatArray(A source) => new(); }",
            "return global::A.ToFloatArray(source);"
        ),
        InlineData(
            "float[]",
            "struct A { public static float[] ToFloatArray(A source) => new(); }",
            "return global::A.ToFloatArray(source);"
        ),
        InlineData(
            "float[]",
            "class  A { public static float[] ToSingleArray(A source) => new(); }",
            "return global::A.ToSingleArray(source);"
        ),
        InlineData(
            "float[]",
            "struct A { public static float[] ToSingleArray(A source) => new(); }",
            "return global::A.ToSingleArray(source);"
        ),
        InlineData("double", "class  A { public static double ToDouble(A source) => new(); }", "return global::A.ToDouble(source);"),
        InlineData("double", "struct A { public static double ToDouble(A source) => new(); }", "return global::A.ToDouble(source);"),
        InlineData(
            "double[]",
            "class  A { public static double[] ToDoubleArray(A source) => new(); }",
            "return global::A.ToDoubleArray(source);"
        ),
        InlineData(
            "double[]",
            "struct A { public static double[] ToDoubleArray(A source) => new(); }",
            "return global::A.ToDoubleArray(source);"
        ),
        InlineData("bool", "class  A { public static bool ToBool(A source) => new(); }", "return global::A.ToBool(source);"),
        InlineData("bool", "struct A { public static bool ToBool(A source) => new(); }", "return global::A.ToBool(source);"),
        InlineData("bool", "class  A { public static bool ToBoolean(A source) => new(); }", "return global::A.ToBoolean(source);"),
        InlineData("bool", "struct A { public static bool ToBoolean(A source) => new(); }", "return global::A.ToBoolean(source);"),
        InlineData("bool[]", "class  A { public static bool[] ToBoolArray(A source) => new(); }", "return global::A.ToBoolArray(source);"),
        InlineData("bool[]", "struct A { public static bool[] ToBoolArray(A source) => new(); }", "return global::A.ToBoolArray(source);"),
        InlineData(
            "bool[]",
            "class  A { public static bool[] ToBooleanArray(A source) => new(); }",
            "return global::A.ToBooleanArray(source);"
        ),
        InlineData(
            "bool[]",
            "struct A { public static bool[] ToBooleanArray(A source) => new(); }",
            "return global::A.ToBooleanArray(source);"
        ),
        InlineData("char", "class  A { public static char ToChar(A source) => new(); }", "return global::A.ToChar(source);"),
        InlineData("char", "struct A { public static char ToChar(A source) => new(); }", "return global::A.ToChar(source);"),
        InlineData("char[]", "class  A { public static char[] ToCharArray(A source) => new(); }", "return global::A.ToCharArray(source);"),
        InlineData("char[]", "struct A { public static char[] ToCharArray(A source) => new(); }", "return global::A.ToCharArray(source);"),
    ]
    public void CustomTypeWithStaticToTargetMethod(string targetType, string classDecl, string expectedResult)
    {
        var source = TestSourceBuilder.Mapping("A", targetType, classDecl);
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody(expectedResult);
    }

    [Fact]
    public void CustomTypeWithStaticToTargetGenericMethod()
    {
        var source = TestSourceBuilder.Mapping("A<byte>", "List<byte>", "class A<T> { public static List<T> ToList(A<T> source) => []; }");
        TestHelper.GenerateMapper(source).Should().HaveSingleMethodBody("return global::A<byte>.ToList(source);");
    }

    [Fact]
    public void DateTimeToDateOnlyMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "DateOnly",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.DateTimeToDateOnly)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void DateTimeToTimeOnlyMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "DateTime",
            "TimeOnly",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.DateTimeToTimeOnly)
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.SourceMemberNotMapped)
            .HaveAssertedAllDiagnostics();
    }

    [Fact]
    public void StaticMethodMappingDisabledShouldDiagnostic()
    {
        var source = TestSourceBuilder.Mapping(
            "A",
            "int",
            TestSourceBuilderOptions.WithDisabledMappingConversion(MappingConversionType.StaticConvertMethods),
            "class A { public static int ToInt32() => 0; }"
        );
        TestHelper
            .GenerateMapper(source, TestHelperOptions.AllowDiagnostics)
            .Should()
            .HaveDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping)
            .HaveAssertedAllDiagnostics();
    }
}
// class A<T> where T: struct { public static List<T?> ToList(A<T> source) => []; }
