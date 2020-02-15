using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPSettingsEditor
{
    public static class UWPSerializer
    {
        public static byte[] FromByte(byte data, DateTimeOffset? timestamp = null)
            => new byte[] { data }.AppendTimestamp(timestamp);
        public static byte[] FromInt16(short data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromUInt16(ushort data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromInt32(int data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromUInt32(uint data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromInt64(long data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromUInt64(ulong data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);


        public static byte[] FromSingle(float data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromDouble(double data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);

        public static byte[] FromChar(char data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);
        public static byte[] FromBoolean(bool data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data).AppendTimestamp(timestamp);


        public static byte[] FromString(string data, DateTimeOffset? timestamp = null)
            => Encoding.Unicode.GetBytes(data).AppendTimestamp(timestamp);

        public static byte[] FromDateTimeOffset(DateTimeOffset data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data.ToFileTime()).AppendTimestamp(timestamp);
        public static byte[] FromTimeSpan(TimeSpan data, DateTimeOffset? timestamp = null)
            => BitConverter.GetBytes(data.Ticks).AppendTimestamp(timestamp);

        public static byte[] FromGuid(Guid data, DateTimeOffset? timestamp = null)
            => data.ToByteArray().AppendTimestamp(timestamp);

        public static byte[] FromPoint(Point data, DateTimeOffset? timestamp = null)
        {
            byte[] x = BitConverter.GetBytes((float)data.X);
            byte[] y = BitConverter.GetBytes((float)data.Y);

            byte[] array = new byte[8];
            Array.Copy(x, 0, array, 0, x.Length);
            Array.Copy(y, 0, array, x.Length, y.Length);

            return array.AppendTimestamp(timestamp);
        }

        public static byte[] FromSize(Size data, DateTimeOffset? timestamp = null)
        {
            byte[] width = BitConverter.GetBytes((float)data.Width);
            byte[] height = BitConverter.GetBytes((float)data.Height);

            byte[] array = new byte[8];
            Array.Copy(width, 0, array, 0, width.Length);
            Array.Copy(height, 0, array, width.Length, height.Length);

            return array.AppendTimestamp(timestamp);
        }

        public static byte[] FromRect(Rect data, DateTimeOffset? timestamp = null)
        {
            byte[] x = BitConverter.GetBytes((float)data.X);
            byte[] y = BitConverter.GetBytes((float)data.Y);
            byte[] width = BitConverter.GetBytes((float)data.Width);
            byte[] height = BitConverter.GetBytes((float)data.Height);

            byte[] array = new byte[16];
            Array.Copy(x, 0, array, 0, x.Length);
            Array.Copy(y, 0, array, 4, y.Length);
            Array.Copy(width, 0, array, 8, width.Length);
            Array.Copy(height, 0, array, 12, height.Length);

            return array.AppendTimestamp(timestamp);
        }

        public static byte[] FromStringArray(string[] data, DateTimeOffset? timestamp = null)
        {
            var totalLength = 0;
            for (int i = 0; i < data.Length; i++)
                totalLength += (data[i].Length * 2) + 2;

            byte[] nullTerminator = new byte[] { 0x0, 0x0 };
            byte[] array = new byte[totalLength];

            var curLengthPosition = 0;

            for (int i = 0; i < data.Length; i++)
            {
                byte[] coded = Encoding.Unicode.GetBytes(data[i]);
                Array.Copy(coded, 0, array, curLengthPosition, coded.Length);
                curLengthPosition += coded.Length;
                Array.Copy(nullTerminator, 0, array, curLengthPosition, 2);
                curLengthPosition += 2;
            }

            FromArray(new float[0], 2, uwu => { return new byte[0]; }, null);

            return array.AppendTimestamp(timestamp);
        }

        public static byte[] FromArray<T>(T[] data, int sizeOfPrimitiveType, Func<T, byte[]> arr, DateTimeOffset? timestamp = null)
        {
            byte[] array = new byte[data.Length * sizeOfPrimitiveType];

            for (int i = 0; i < data.Length; i++)
            {
                var res = arr(data[i]);
                Array.Copy(res, 0, array, sizeOfPrimitiveType * i, sizeOfPrimitiveType);
            }

            return array.AppendTimestamp(timestamp);
        }

        private static byte[] FromDateTimeOffset(DateTimeOffset data)
            => BitConverter.GetBytes(data.ToFileTime());

        private static byte[] AppendTimestamp(this byte[] data, DateTimeOffset? timestamp)
        {
            //No timestamp? don't continue
            if (!timestamp.HasValue)
                return data;

            byte[] timestampSerialized = FromDateTimeOffset(timestamp.Value);

            byte[] prependedArray = new byte[data.Length + timestampSerialized.Length];
            Array.Copy(data, 0, prependedArray, 0, data.Length);
            Array.Copy(timestampSerialized, 0, prependedArray, data.Length, timestampSerialized.Length);
            
            return prependedArray;
        }
    }
}
