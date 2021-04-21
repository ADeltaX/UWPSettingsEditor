using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPSettingsEditor
{
    public static class UWPDeserializer
    {
        public static byte GetByte(byte[] data, int index = 0) 
            => data[index];

        public static short GetInt16(byte[] data, int index = 0) 
            => BitConverter.ToInt16(data, index);
        public static ushort GetUInt16(byte[] data, int index = 0) 
            => BitConverter.ToUInt16(data, index);
        public static int GetInt32(byte[] data, int index = 0) 
            => BitConverter.ToInt32(data, index);
        public static uint GetUInt32(byte[] data, int index = 0) 
            => BitConverter.ToUInt32(data, index);
        public static long GetInt64(byte[] data, int index = 0) 
            => BitConverter.ToInt64(data, index);
        public static ulong GetUInt64(byte[] data, int index = 0) 
            => BitConverter.ToUInt64(data, index);

        public static float GetSingle(byte[] data, int index = 0) 
            => BitConverter.ToSingle(data, index);
        public static double GetDouble(byte[] data, int index = 0) 
            => BitConverter.ToDouble(data, index);

        public static char GetChar(byte[] data, int index = 0) 
            => BitConverter.ToChar(data, index);
        public static bool GetBoolean(byte[] data, int index = 0) 
            => BitConverter.ToBoolean(data, index);

        public static string GetString(byte[] data) 
            => Encoding.Unicode.GetString(data);
        public static string GetString(byte[] data, int index, int length)
            => Encoding.Unicode.GetString(data, index, length);


        public static DateTimeOffset GetDateTimeOffset(byte[] data, int index = 0) 
            => DateTimeOffset.FromFileTime(BitConverter.ToInt64(data, index));
        public static TimeSpan GetTimeSpan(byte[] data, int index = 0)
            => new TimeSpan(BitConverter.ToInt64(data, index));

        public static Guid GetGuid(byte[] data) 
            => new Guid(data);
        public static Guid GetGuid(byte[] data, int index) 
            => new Guid(data.Skip(index).Take(16).ToArray());

        public static Point GetPoint(byte[] data, int index = 0)
        {
            //struct of 2 singles (it should be double, but this is a nice bug. Try using double.maxvalue in your uwp code, save it, and then read from it. Infinity.)
            double x = BitConverter.ToSingle(data, index + 0);
            double y = BitConverter.ToSingle(data, index + 4);

            return new Point(x, y);
        }

        public static Size GetSize(byte[] data, int index = 0)
        {
            double width = BitConverter.ToSingle(data, index + 0);
            double height = BitConverter.ToSingle(data, index + 4);

            return new Size(width, height);
        }
        
        public static Rect GetRect(byte[] data, int index = 0)
        {
            double x = BitConverter.ToSingle(data, index + 0);
            double y = BitConverter.ToSingle(data, index + 4);
            double width = BitConverter.ToSingle(data, index + 8);
            double height = BitConverter.ToSingle(data, index + 12);

            return new Rect(x, y, width, height);
        }

        public static T[] GetArray<T>(int length, int sizeOfPrimitiveType, Func<int, T> arr)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < length; i += sizeOfPrimitiveType)
                list.Add(arr(i));

            return list.ToArray();
        }

        public static T[] GetArray<T>(int index, int length, int sizeOfPrimitiveType, Func<int, T> arr)
        {
            List<T> list = new List<T>();
            for (int i = index; i < length + index; i += sizeOfPrimitiveType)
                list.Add(arr(i));

            return list.ToArray();
        }

        public static string[] GetStringArray(int index, int length, byte[] data)
        {
            List<string> strings = new List<string>();

            for (int position = index; position < index + length;)
            {
                var stringLength = BitConverter.ToInt32(data, position);
                var indexpos = position + 4;
                position = position + 4 + stringLength;


                strings.Add(GetString(data, indexpos, stringLength - 2)); //minus null-terminator
            }

            return strings.ToArray();
        }

        public static Dictionary<string, object> GetCompositeValue(byte[] data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            int currentPos = 0;

            do
            {
                var compositeLength = GetInt32(data, currentPos);
                var compositeTypeData = (DataTypeEnum)((int)(DataTypeEnum.RegUwpByte) + GetUInt32(data, currentPos + 4) - 1);
                var compositeKeyLength = GetInt32(data, currentPos + 8) * 2 + 2; //* sizeof wchar + null-termination

                var padding = 8 - (compositeLength % 8);

                if (padding == 8)
                    padding = 0;

                var keyStr = GetString(data, currentPos + 12, compositeKeyLength - 2); //we don't need null-termination bc it's not winrt hstring

                object value = null;

                currentPos += 12 + compositeKeyLength; //header (4 byte * 3) + compositeKeyLength
                var valueLength = compositeLength - compositeKeyLength - 12; //minus header

                switch (compositeTypeData)
                {
                    case DataTypeEnum.RegUwpByte:
                        value = GetByte(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpInt16:
                        value = GetInt16(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpUint16:
                        value = GetUInt16(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpInt32:
                        value = GetInt32(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpUint32:
                        value = GetUInt32(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpInt64:
                        value = GetInt64(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpUint64:
                        value = GetUInt64(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpSingle:
                        value = GetSingle(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpDouble:
                        value = GetDouble(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpChar:
                        value = GetChar(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpBoolean:
                        value = GetBoolean(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpString:
                        value = GetString(data, currentPos, valueLength - 2); //minus null-termination
                        break;
                    case DataTypeEnum.RegUwpDateTimeOffset:
                        value = GetDateTimeOffset(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpTimeSpan:
                        value = GetTimeSpan(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpGuid:
                        value = GetGuid(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpPoint:
                        value = GetPoint(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpSize:
                        value = GetSize(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpRect:
                        value = GetRect(data, currentPos);
                        break;
                    case DataTypeEnum.RegUwpArrayByte:
                        value = GetArray(currentPos, valueLength, 1, i => GetByte(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayInt16:
                        value = GetArray(currentPos, valueLength, 2, i => GetInt16(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayUint16:
                        value = GetArray(currentPos, valueLength, 2, i => GetUInt16(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayInt32:
                        value = GetArray(currentPos, valueLength, 4, i => GetInt32(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayUint32:
                        value = GetArray(currentPos, valueLength, 4, i => GetUInt32(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayInt64:
                        value = GetArray(currentPos, valueLength, 8, i => GetInt64(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayUint64:
                        value = GetArray(currentPos, valueLength, 8, i => GetUInt64(data, i));
                        break;
                    case DataTypeEnum.RegUwpArraySingle:
                        value = GetArray(currentPos, valueLength, 4, i => GetSingle(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayDouble:
                        value = GetArray(currentPos, valueLength, 8, i => GetDouble(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayChar16:
                        value = GetArray(currentPos, valueLength, 2, i => GetChar(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayBoolean:
                        value = GetArray(currentPos, valueLength, 1, i => GetBoolean(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayString:
                        value = GetStringArray(currentPos, valueLength, data);
                        break;
                    case DataTypeEnum.RegUwpArrayDateTimeOffset:
                        value = GetArray(currentPos, valueLength, 8, i => GetDateTimeOffset(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayTimeSpan:
                        value = GetArray(currentPos, valueLength, 8, i => GetTimeSpan(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayGuid:
                        value = GetArray(currentPos, valueLength, 16, i => GetGuid(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayPoint:
                        value = GetArray(currentPos, valueLength, 4 * 2, i => GetPoint(data, i));
                        break;
                    case DataTypeEnum.RegUwpArraySize:
                        value = GetArray(currentPos, valueLength, 4 * 2, i => GetSize(data, i));
                        break;
                    case DataTypeEnum.RegUwpArrayRect:
                        value = GetArray(currentPos, valueLength, 4 * 4, i => GetRect(data, i));
                        break;
                    default:
                        break;
                }
                
                if (value != null)
                    dict.Add(keyStr, value);

                currentPos += valueLength + padding;

            } while (currentPos < data.Length);

            return dict;
        }

        public static string PrettyPrintDictionary(Dictionary<string, object> dict)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int i = 0;
            foreach (KeyValuePair<string, object> item in dict)
            {
                var formatted = item.Value;

                if (item.Value is string)
                    formatted = "\"" + item.Value + "\"";
                else if (item.Value is char)
                    formatted = "'" + item.Value + "'";
                else if (item.Value is char[])
                    formatted = "[ " + PrettyPrintArray(item.Value as Array, "'") + " ]";
                else if (item.Value is string[])
                    formatted = "[ " + PrettyPrintArray(item.Value as Array, "\"") + " ]";
                else if (item.Value is Array)
                    formatted = "[ " + PrettyPrintArray(item.Value as Array) + " ]";

                stringBuilder.Append("{ \"" + item.Key + "\" : " + formatted + " }");

                if (++i < dict.Count)
                    stringBuilder.Append(", ");
            }

            return stringBuilder.ToString();
        }

        public static string PrettyPrintArray(Array array, string quotes = "")
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append("[ " + quotes + array.GetValue(i).ToString() + quotes + " ]");
                if (i < array.Length - 1)
                    stringBuilder.Append(", ");
            }

            return stringBuilder.ToString();
        }

        public static string PrettyPrintArray(byte[] array, string quotes = "")
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("X2"));
                if (i < array.Length - 1)
                    stringBuilder.Append(" ");
            }

            return stringBuilder.ToString();
        }

        public static string PrettyPrintStringArrayFromRaw(byte[] dataRaw)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int position = 0; position < dataRaw.Length;)
            {
                var stringLength = BitConverter.ToInt32(dataRaw, position);
                var index = position + 4;
                position = position + 4 + stringLength;


                stringBuilder.Append("[ \"" + GetString(dataRaw, index, stringLength).ReplaceMultilineWithSymbols() + "\" ]");
                if (position != dataRaw.Length)
                    stringBuilder.Append(", ");
            }

            return stringBuilder.ToString();
        }
    }

    public static class Deserializer
    {
        public static uint GetUInt32(byte[] data, int index = 0)
            => BitConverter.ToUInt32(data, index);

        public static uint GetUInt32BigEndian(byte[] data, int index = 0)
            => (uint)((data[index] << 24) | (data[index + 1] << 16) | (data[index + 2] << 8) | data[index + 3]);

            public static ulong GetUInt64(byte[] data, int index = 0)
            => BitConverter.ToUInt64(data, index);
    }
}
