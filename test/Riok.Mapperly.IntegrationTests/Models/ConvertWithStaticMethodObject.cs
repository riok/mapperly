using System;

namespace Riok.Mapperly.IntegrationTests.Models
{
    public class ConvertWithStaticMethodObject
    {
        public int Value { get; private set; }

        public static ConvertWithStaticMethodObject Create(int value)
        {
            return new ConvertWithStaticMethodObject { Value = value };
        }

        public static ConvertWithStaticMethodObject CreateFrom(byte value)
        {
            return new ConvertWithStaticMethodObject { Value = value };
        }

        public static ConvertWithStaticMethodObject FromSingle(float value)
        {
            return new ConvertWithStaticMethodObject { Value = Convert.ToInt32(value) };
        }

        public static ConvertWithStaticMethodObject Create(params double[] value)
        {
            return new ConvertWithStaticMethodObject { Value = Convert.ToInt32(value[0]) };
        }

        public static ConvertWithStaticMethodObject CreateFrom(params uint[] value)
        {
            return new ConvertWithStaticMethodObject { Value = Convert.ToInt32(value[0]) };
        }

        public static ConvertWithStaticMethodObject FromInt16(params short[] value)
        {
            return new ConvertWithStaticMethodObject { Value = Convert.ToInt32(value[0]) };
        }

        public static int ToInt32(ConvertWithStaticMethodObject obj)
        {
            return obj.Value;
        }

        public static decimal ToDecimal(ConvertWithStaticMethodObject obj)
        {
            return obj.Value;
        }

        public static byte ToByte(ConvertWithStaticMethodObject obj)
        {
            return Convert.ToByte(obj.Value);
        }

        public static float ToSingle(ConvertWithStaticMethodObject obj)
        {
            return Convert.ToSingle(obj.Value);
        }

        public static double ToDouble(ConvertWithStaticMethodObject obj)
        {
            return Convert.ToDouble(obj.Value);
        }

        public static uint ToUInt32(ConvertWithStaticMethodObject obj)
        {
            return Convert.ToUInt32(obj.Value);
        }

        public static short ToInt16(ConvertWithStaticMethodObject obj)
        {
            return Convert.ToInt16(obj.Value);
        }
    }
}
